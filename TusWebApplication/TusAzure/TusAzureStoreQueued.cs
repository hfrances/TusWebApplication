using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Interfaces;

namespace TusWebApplication.TusAzure
{
    sealed class TusAzureStoreQueued : TusAzureStore, ITusStore, ITusCreationStore, ITusTerminationStore, ITusReadableStore
    {

        public TusAzureStoreQueued(string accountName, string accountKey, string defaultContainer)
            : base(accountName, accountKey, defaultContainer)
        { }

        public TusAzureStoreQueued(Azure.Storage.Blobs.BlobServiceClient blobService, string defaultContainer)
            : base(blobService, defaultContainer)
        { }

        public override async Task<long> AppendDataAsync(string fileId, Stream stream, CancellationToken cancellationToken)
        {

            try
            {
                var threadId = Guid.NewGuid().ToString()[..8];
                var blobInfo = Blobs[fileId];
                var blockId = $"{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(blobInfo.BlockNames.Count.ToString("d6")))}";
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
                blobInfo.QueueCount += 1;
                stream.DisposeAsync().GetAwaiter();
                memo = null;

                if (!blobInfo.Queue.Any(x => x.Status == QueueItemStatus.Started)) // Si la cola no está en curso, iniciarla.
                {
                    _ = Task.Run(async () =>
                      {
                          try
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
                                          blobInfo.QueuePosition += 1;
                                          Console.WriteLine($"FileId: {blobInfo.FileId}. ThreadId: {threadId}. BlockId: {block.Name}. Queue item: {blobInfo.QueuePosition}/{blobInfo.QueueCount}");

                                          // Calculate MD5 block.
                                          var buffer = new byte[block.Length];
                                          int readed;
                                          readed = await block.Stream.ReadAsync(buffer.AsMemory(0, (int)block.Length), cancellationToken);
                                          blobInfo.Hasher.TransformBlock(buffer, 0, readed, null, 0);
                                          block.Stream.Position = 0;

                                          // Upload block.
                                          _ = await blobInfo.Blob.StageBlockAsync(block.Name, block.Stream, cancellationToken: cancellationToken);
                                          blobInfo.SizeOffsetInternal += length;

                                          // End
                                          block.Status = QueueItemStatus.Done;
                                          block.Dispose();
                                          blobInfo.Queue.Remove(block);
                                          Console.WriteLine($"FileId: {blobInfo.FileId}. ThreadId: {threadId}. BlockId: {block.Name}. Done.");
                                          GC.Collect();
                                      }
                                  }
                              }
                              catch (Exception ex)
                              {
                                  Console.WriteLine($"FileId: {blobInfo.FileId}. ThreadId: {threadId}. BlockId: {blockId}. ERROR: {ex.Message}. Elapsed time: {DateTime.Now - blobInfo.StartTime.Value}");
                                  throw;
                              }

                              if (blobInfo.SizeOffset == blobInfo.UploadLength)
                              {


                                  blobInfo.Hasher.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

                                  // Commit.
                                  var commitOptions = TusAzureHelper.CreateCommitBlockListOptions(blobInfo);
                                  var contentHash = commitOptions.HttpHeaders.ContentHash;

                                  Console.WriteLine($"FileId: {blobInfo.FileId}. ThreadId: {threadId}. Hash: {Convert.ToBase64String(contentHash ?? Array.Empty<byte>())}. Commiting...");
                                  await blobInfo.Blob.CommitBlockListAsync(blobInfo.BlockNames, commitOptions, cancellationToken: cancellationToken);
                                  Console.WriteLine($"FileId: {blobInfo.FileId}. ThreadId: {threadId}. Commited. Elapsed time: {DateTime.Now - blobInfo.StartTime.Value}");

                                  // Validate.
                                  var container = BlobService.GetBlobContainerClient(blobInfo.ContainerName);
                                  var blob = container.GetBlobClient(blobInfo.BlobName);
                                  blob.GetHashCode();
                                  var uri = blob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(12));
                                  uri.ToString();
                              }
                          }
                          catch (Exception ex)
                          {
                              Console.WriteLine($"FileId: {blobInfo.FileId}. ThreadId: {threadId}. ERROR: {ex.Message}. Elapsed time: {DateTime.Now - blobInfo.StartTime.Value}");
                              throw;
                          }
                          finally
                          {
                              blobInfo.Dispose();
                              Blobs.Remove(blobInfo.FileId);
                              GC.Collect();
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

    }
}
