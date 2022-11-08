using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Interfaces;
using tusdotnet.Models;

namespace TusWebApplication
{
    sealed class TusAzureStoreQueued : ITusStore, ITusCreationStore, ITusTerminationStore, ITusReadableStore
    {

        private enum QueueItemStatus
        {
            Waiting, Started, Done
        }

        private class QueueItem
        {
            public string Name { get; }
            public Stream? Stream { get; set; }
            public long Length { get; set; }
            public QueueItemStatus Status { get; set; }

            public QueueItem(string name, Stream stream, long length)
            {
                this.Name = name;
                this.Stream = stream;
                this.Length = length;
            }

        }

        private class BlobInfo
        {
            public string FileId { get; }
            public string ContainerName { get; }
            public string Metadata { get; }
            public long UploadLength { get; }
            public BlockBlobClient Blob { get; }
            public IList<string> BlockNames { get; }
                = new List<string>();
            public IList<QueueItem> Queue { get; }
                = new List<QueueItem>();
            public long SizeOffset { get; set; }
            public long SizeOffsetInternal { get; set; }
            public DateTime? StartTime { get; set; }
            public HashAlgorithm Hasher { get; }

            public BlobInfo(string fileId, string containerName, string metadata, long uploadLength, BlockBlobClient blob)
            {
                this.FileId = fileId;
                this.ContainerName = containerName;
                this.Metadata = metadata;
                this.UploadLength = uploadLength;
                this.Blob = blob;

                this.Hasher = MD5.Create();
                Hasher.Initialize();
            }

        }

        Azure.Storage.Blobs.BlobServiceClient BlobService { get; }
        string DefaultContainer { get; }
        Dictionary<string, BlobInfo> Blobs { get; } = new Dictionary<string, BlobInfo>();


        public TusAzureStoreQueued(string accountName, string accountKey, string defaultContainer)
        {
            var credentials = new Azure.Storage.StorageSharedKeyCredential(accountName, accountKey);
            var blobUri = new Uri($"https://{accountName}.blob.core.windows.net");

            this.BlobService = new Azure.Storage.Blobs.BlobServiceClient(blobUri, credentials);
            this.DefaultContainer = defaultContainer;
        }

        public TusAzureStoreQueued(Azure.Storage.Blobs.BlobServiceClient blobService, string defaultContainer)
        {
            this.BlobService = blobService;
            this.DefaultContainer = defaultContainer;
        }

        public async Task<long> AppendDataAsync(string fileId, Stream stream, CancellationToken cancellationToken)
        {

            try
            {
                var threadId = Guid.NewGuid().ToString()[..8];
                var blobInfo = Blobs[fileId];
                var blockId = $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(blobInfo.BlockNames.Count.ToString("d6")))}";
                long length = 0;

                // Iniciar el contador de tiempo.
                if (blobInfo.StartTime == null)
                {
                    blobInfo.StartTime = DateTime.Now;
                }

                // Copiar contenido a un MemoryStream. El Stream que devuelve este método es muy limitado.
                var memo = new MemoryStream();
                await stream.CopyToAsync(memo, cancellationToken);
                memo.Position = 0;
                length = memo.Length;
                blobInfo.Queue.Add(new QueueItem(blockId, memo, length));
                blobInfo.BlockNames.Add(blockId);
                blobInfo.SizeOffset += length;

                if (!blobInfo.Queue.Any(x => x.Status == QueueItemStatus.Started)) // Si la cola no está en curso, iniciarla.
                {
                    _ = Task.Run(async () =>
                      {
                          try
                          {
                              QueueItem? block;

                              // Una vez entrado en el bucle, seguirá hasta que no quede nada más en la cola.
                              // Las demás llamadas a este método simplemente añadirán en la cola.
                              while ((block = blobInfo.Queue.FirstOrDefault(x => x.Status == QueueItemStatus.Waiting)) != null)
                              {
                                  if (block.Stream == null)
                                  {
                                      throw new NullReferenceException("There is no stream content in this block.");
                                  }
                                  else
                                  {
                                      // Begin
                                      block.Status = QueueItemStatus.Started;
                                      Console.WriteLine($"FileId: {blobInfo.FileId}. ThreadId: {threadId}. BlockId: {block.Name}. Queue item: {blobInfo.Queue.IndexOf(block) + 1}/{blobInfo.Queue.Count}");

                                      // Calculate MD5 block.
                                      var buffer = new byte[block.Length];
                                      int length;
                                      length = await block.Stream.ReadAsync(buffer.AsMemory(0, (int)block.Length));
                                      blobInfo.Hasher.TransformBlock(buffer, 0, length, null, 0);
                                      block.Stream.Position = 0;

                                      // Upload block.
                                      _ = await blobInfo.Blob.StageBlockAsync(block.Name, block.Stream, cancellationToken: cancellationToken);
                                      blobInfo.SizeOffsetInternal += length;

                                      // End
                                      block.Stream?.Dispose();
                                      block.Stream = null;
                                      block.Status = QueueItemStatus.Done;
                                      Console.WriteLine($"FileId: {blobInfo.FileId}. ThreadId: {threadId}. BlockId: {block.Name}. Done.");
                                  }
                              }
                          }
                          catch (Exception)
                          {
                              throw;
                          }

                          try
                          {
                              if (blobInfo.SizeOffset == blobInfo.UploadLength)
                              {
                                  blobInfo.Hasher.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                                  var contentHash = blobInfo.Hasher.Hash;

                                  Console.WriteLine($"FileId: {blobInfo.FileId}. ThreadId: {threadId}. Hash: {Convert.ToBase64String(contentHash ?? new byte[] { })}. Commiting...");

                                  var commitOptions = new Azure.Storage.Blobs.Models.CommitBlockListOptions()
                                  {
                                      Tags = new Dictionary<string, string>(),
                                      Metadata = new Dictionary<string, string>(),
                                      HttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders()
                                      {
                                          ContentHash = contentHash
                                      }
                                  };
                                  var metadataParsed = tusdotnet.Parsers.MetadataParser.ParseAndValidate(MetadataParsingStrategy.AllowEmptyValues, blobInfo.Metadata).Metadata;
                                  var fileName = metadataParsed["filename"].GetString(System.Text.Encoding.UTF8);
                                  var container = metadataParsed["container"].GetString(System.Text.Encoding.UTF8);
                                  var factor = metadataParsed["factor"].GetString(System.Text.Encoding.UTF8);

                                  commitOptions.Tags.Add("filename", fileName);
                                  commitOptions.Metadata.Add("factor", factor);
                                  await blobInfo.Blob.CommitBlockListAsync(blobInfo.BlockNames, commitOptions, cancellationToken: cancellationToken);
                                  Console.WriteLine($"FileId: {blobInfo.FileId}. ThreadId: {threadId}. Commited. Elapsed time: {DateTime.Now - blobInfo.StartTime.Value}");

                                  var container2 = BlobService.GetBlobContainerClient(blobInfo.ContainerName);
                                  var blob = container2.GetBlobClient(blobInfo.FileId);
                              }
                          }
                          catch (Exception)
                          {
                              throw;
                          }
                      }, cancellationToken);
                }
                return await Task.FromResult(length);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<string> CreateFileAsync(long uploadLength, string metadata, CancellationToken cancellationToken)
        {
            var metadataParsed = tusdotnet.Parsers.MetadataParser.ParseAndValidate(MetadataParsingStrategy.AllowEmptyValues, metadata)?.Metadata;
            string containerName;

            if (metadataParsed?.TryGetValue("container", out Metadata? value) == true)
            {
                containerName = value.GetString(System.Text.Encoding.UTF8);
            }
            else
            {
                containerName = this.DefaultContainer;
            }

            var container = BlobService.GetBlobContainerClient(containerName);

            if (container == null)
            {
                throw new Exception($"Container with name '{containerName}' not found.");
            }
            else
            {
                var blobName = Guid.NewGuid().ToString();
                var blobId = $"{containerName}/{blobName}";
                var blob = container.GetBlockBlobClient(blobName);

                Blobs.Add(blobId, new BlobInfo(blobId, containerName, metadata, uploadLength, blob));
                return Task.FromResult(blobId);
            }
        }

        public Task<bool> FileExistAsync(string fileId, CancellationToken cancellationToken)
        {
            bool rdo;

            if (Blobs.TryGetValue(fileId, out BlobInfo? blobInfo))
            {
                var container = BlobService.GetBlobContainerClient(blobInfo.ContainerName);
                var blob = container.GetBlobClient(fileId);

                rdo = (blob != null);
            }
            else
            {
                rdo = false;
            }
            return Task.FromResult(rdo);
        }

        public Task<long?> GetUploadLengthAsync(string fileId, CancellationToken cancellationToken)
        {
            return Task.FromResult((long?)Blobs[fileId].UploadLength);
        }

        public Task<string> GetUploadMetadataAsync(string fileId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Blobs[fileId].Metadata);
        }

        public Task<long> GetUploadOffsetAsync(string fileId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Blobs[fileId].SizeOffset);
        }

        public Task<ITusFile> GetFileAsync(string fileId, CancellationToken cancellationToken)
        {
            var blobId = fileId.Split('/');

            if (blobId.Length == 2)
            {
                var containerName = blobId[0];
                var blobName = blobId[1];
                var container = BlobService.GetBlobContainerClient(containerName);

                if (container == null)
                {
                    throw new KeyNotFoundException($"Container '{containerName}' not found in blob storage.");
                }
                else
                {
                    var blob = container.GetBlobClient(blobName);

                    if (blob == null)
                    {
                        throw new KeyNotFoundException($"Blob '{blobName}' not found in container '{containerName}'.");
                    }
                    else
                    {
                        var file = new TusAzureFile(blob);

                        return Task.FromResult<ITusFile>(file);
                    }
                }
            }
            else
            {
                throw new FormatException("Invalid format for FileId.");
            }
        }

        public Task DeleteFileAsync(string fileId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }
}
