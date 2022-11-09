using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Interfaces;

namespace TusWebApplication.TusAzure
{
    internal class TusAzureStore : ITusStore, ITusCreationStore, ITusTerminationStore, ITusReadableStore
    {

        protected Azure.Storage.Blobs.BlobServiceClient BlobService { get; }
        protected string DefaultContainer { get; }
        protected Dictionary<string, BlobInfo> Blobs { get; } = new Dictionary<string, BlobInfo>();


        public TusAzureStore(string accountName, string accountKey, string defaultContainer)
        {
            var credentials = new Azure.Storage.StorageSharedKeyCredential(accountName, accountKey);
            var blobUri = new Uri($"https://{accountName}.blob.core.windows.net");

            this.BlobService = new Azure.Storage.Blobs.BlobServiceClient(blobUri, credentials);
            this.DefaultContainer = defaultContainer;
        }

        public TusAzureStore(Azure.Storage.Blobs.BlobServiceClient blobService, string defaultContainer)
        {
            this.BlobService = blobService;
            this.DefaultContainer = defaultContainer;
        }

        public virtual async Task<long> AppendDataAsync(string fileId, Stream stream, CancellationToken cancellationToken)
        {
            var blobInfo = Blobs[fileId];

            // Iniciar el contador de tiempo.
            if (blobInfo.StartTime == null)
            {
                blobInfo.StartTime = DateTime.Now;
            }

            // Procesar bloque.
            try
            {
                var blockId = $"{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(blobInfo.BlockNames.Count.ToString("d6")))}";
                long length = 0;

                blobInfo.QueueCount += 1;
                blobInfo.QueuePosition += 1;
                using (var memo = new MemoryStream())
                {
                    Console.WriteLine($"FileId: {blobInfo.FileId}. BlockId: {blockId}. Queue item: {blobInfo.QueuePosition}/{blobInfo.QueueCount}");

                    await stream.CopyToAsync(memo, cancellationToken);
                    memo.Position = 0;
                    length = memo.Length;

                    // Calculate MD5 block.
                    var buffer = new byte[length];
                    int readed;
                    readed = await stream.ReadAsync(buffer.AsMemory(0, (int)length), cancellationToken);
                    blobInfo.Hasher.TransformBlock(buffer, 0, readed, null, 0);
                    memo.Position = 0;

                    // Upload block.
                    _ = await blobInfo.Blob.StageBlockAsync(blockId, memo, cancellationToken: cancellationToken);
                    blobInfo.SizeOffset += length;
                    blobInfo.BlockNames.Add(blockId);
                    Console.WriteLine($"FileId: {blobInfo.FileId}. BlockId: {blockId}. Done.");
                    GC.Collect();
                }
                if (blobInfo.SizeOffset == blobInfo.UploadLength)
                {
                    blobInfo.Hasher.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

                    // Commit
                    var commitOptions = TusAzureHelper.CreateCommitBlockListOptions(blobInfo);
                    var contentHash = commitOptions.HttpHeaders.ContentHash;

                    Console.WriteLine($"FileId: {blobInfo.FileId}. Hash: {Convert.ToBase64String(contentHash ?? Array.Empty<byte>())}. Commiting...");
                    await blobInfo.Blob.CommitBlockListAsync(blobInfo.BlockNames, commitOptions, cancellationToken: cancellationToken);
                    Console.WriteLine($"FileId: {blobInfo.FileId}. Commited. Elapsed time: {DateTime.Now - blobInfo.StartTime.Value}");
                }
                return length;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FileId: {blobInfo.FileId}. ERROR: {ex.Message}. Elapsed time: {DateTime.Now - blobInfo.StartTime.Value}");
                throw;
            }
            finally
            {
                blobInfo.Dispose();
            }
        }

        public Task<string> CreateFileAsync(long uploadLength, string metadata, CancellationToken cancellationToken)
        {
            var container = TusAzureHelper.GetContainer(this.BlobService, metadata, this.DefaultContainer);
            var blobName = Guid.NewGuid().ToString();
            var blobId = $"{container.Name}/{blobName}";
            var blob = container.GetBlockBlobClient(blobName);

            Blobs.Add(blobId, new BlobInfo(blobId, container.Name, blobName, metadata, uploadLength, blob));
            return Task.FromResult(blobId);
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
