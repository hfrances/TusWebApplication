using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
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

        public TusAzureStoreQueued(string storeName, string accountName, string accountKey, string defaultContainer, IHttpContextAccessor httpContextAccessor, ILogger logger)
            : base(storeName, accountName, accountKey, defaultContainer, httpContextAccessor, logger)
        { }

        public TusAzureStoreQueued(string storeName, Azure.Storage.Blobs.BlobServiceClient blobService, string defaultContainer, IHttpContextAccessor httpContextAccessor, ILogger logger)
            : base(storeName, blobService, defaultContainer, httpContextAccessor, logger)
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
                memo = null;
                stream.DisposeAsync().GetAwaiter();

                if (blobInfo.UseQueueAsync)
                {
                    if (!blobInfo.Queue.Any(x => x.Status == QueueItemStatus.Started)) // Si la cola no está en curso, iniciarla.
                    {
                        _ = Task.Run(async () =>
                        {
                            await UploadChunk(blobInfo, threadId, blockId, length, cancellationToken);
                        }, cancellationToken);
                    }
                }
                else
                {
                    await UploadChunk(blobInfo, threadId, blockId, length, cancellationToken);
                }
                return await Task.FromResult(length);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task UploadChunk(BlobInfo blobInfo, string threadId, string blockId, long length, CancellationToken cancellationToken)
        {
            if (blobInfo.Blob == null) throw new NullReferenceException("Blob client null");

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
                        Logger.LogInformation($"FileId: {this.StoreName}/{blobInfo.FileId}. ThreadId: {threadId}. BlockId: {block.Name}. Queue item: {blobInfo.QueuePosition}/{blobInfo.QueueCount}");

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
                        Logger.LogInformation($"FileId: {this.StoreName}/{blobInfo.FileId}. ThreadId: {threadId}. BlockId: {block.Name}. Done.");
                        GC.Collect();
                    }
                }
            }
            catch (Exception ex)
            {
                blobInfo.Done = true;
                blobInfo.Error = ex;
                blobInfo.Dispose();
                Logger.LogInformation(ex, $"FileId: {this.StoreName}/{blobInfo.FileId}. ThreadId: {threadId}. BlockId: {blockId}. ERROR: {ex.Message}. Elapsed time: {DateTime.Now - blobInfo.StartTime}");
                throw;
            }
            if (blobInfo.SizeOffset == blobInfo.UploadLength)
            {

                try
                {
                    blobInfo.Hasher.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

                    // Commit.
                    var commitOptions = TusAzureHelper.CreateCommitBlockListOptions(blobInfo);
                    var contentHash = commitOptions.HttpHeaders.ContentHash;
                    var hash = Convert.ToBase64String(contentHash ?? Array.Empty<byte>());

                    Logger.LogInformation($"FileId: {this.StoreName}/{blobInfo.FileId}. ThreadId: {threadId}. Hash: {hash}. Commiting...");
                    if (!string.IsNullOrEmpty(blobInfo.ValidateHash) && blobInfo.ValidateHash != hash)
                    {
                        throw new ArgumentException($"Hash in the request token is different from the uploaded file.");
                    }
                    else
                    {
                        await blobInfo.Blob.CommitBlockListAsync(blobInfo.BlockNames, commitOptions, cancellationToken: cancellationToken);
                        Logger.LogInformation($"FileId: {this.StoreName}/{blobInfo.FileId}. ThreadId: {threadId}. Commited. Elapsed time: {DateTime.Now - blobInfo.StartTime}");

                        // Validate.
                        var container = BlobService.GetBlobContainerClient(blobInfo.ContainerName);
                        var blob = container.GetBlobClient(blobInfo.BlobName);
                        blob.GetHashCode();
                        var uri = blob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(12));
                        uri.ToString();
                        Blobs.Remove(blobInfo.FileId); // Solamente quitar si fue todo bien. En caso contrario se quedará a modo de histórico.
                    }
                }
                catch (Exception ex)
                {
                    blobInfo.Error = ex;
                    Logger.LogError(ex, $"FileId: {this.StoreName}/{blobInfo.FileId}. ThreadId: {threadId}. ERROR: {ex.Message}. Elapsed time: {DateTime.Now - blobInfo.StartTime}");
                    throw;
                }
                finally
                {
                    blobInfo.Done = true;
                    blobInfo.Dispose();
                    GC.Collect();
                }
            }
        }

    }
}
