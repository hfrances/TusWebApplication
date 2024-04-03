using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Interfaces;
using tusdotnet.Models;

namespace TusWebApplication.TusAzure
{
    internal class TusAzureStore : ITusStore, ITusCreationStore, ITusTerminationStore, ITusReadableStore
    {

        protected Azure.Storage.Blobs.BlobServiceClient BlobService { get; }
        protected string DefaultContainer { get; }
        protected IHttpContextAccessor HttpContextAccessor { get; }
        protected ILogger Logger { get; }

        public Dictionary<string, BlobInfo> Blobs { get; } = new Dictionary<string, BlobInfo>();


        public TusAzureStore(string storeName, string accountName, string accountKey, string defaultContainer, IHttpContextAccessor httpContextAccessor, ILogger logger)
        {
            this.StoreName = storeName;
            this.BlobService = TusAzureHelper.CreateBlobServiceClient(accountName, accountKey);
            this.DefaultContainer = defaultContainer;
            this.HttpContextAccessor = httpContextAccessor;
            this.Logger = logger;
        }

        public TusAzureStore(string storeName, Azure.Storage.Blobs.BlobServiceClient blobService, string defaultContainer, IHttpContextAccessor httpContextAccessor, ILogger logger)
        {
            this.StoreName = storeName;
            this.BlobService = blobService;
            this.DefaultContainer = defaultContainer;
            this.HttpContextAccessor = httpContextAccessor;
            this.Logger = logger;
        }


        public string StoreName { get; }

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

                if (blobInfo.Blob == null)
                {
                    throw new NullReferenceException("Blob object is null.");
                }
                else
                {
                    var blockId = $"{Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(blobInfo.BlockNames.Count.ToString("d6")))}";
                    long length = 0;

                    blobInfo.QueueCount += 1;
                    blobInfo.QueuePosition += 1;
                    using (var memo = new MemoryStream())
                    {
                        Logger.LogInformation($"FileId: {this.StoreName}/{blobInfo.FileId}. BlockId: {blockId}. Queue item: {blobInfo.QueuePosition}/{blobInfo.QueueCount}");

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
                        Logger.LogInformation($"FileId: {this.StoreName}/{blobInfo.FileId}. BlockId: {blockId}. Done.");
                    }
                    if (blobInfo.SizeOffset == blobInfo.UploadLength)
                    {
                        try
                        {
                            blobInfo.Hasher.TransformFinalBlock(Array.Empty<byte>(), 0, 0);

                            // Commit
                            var commitOptions = TusAzureHelper.CreateCommitBlockListOptions(blobInfo);
                            var contentHash = commitOptions.HttpHeaders.ContentHash;
                            var hash = Convert.ToBase64String(contentHash ?? Array.Empty<byte>());

                            Logger.LogInformation($"FileId: {this.StoreName}/{blobInfo.FileId}. Hash: {hash}. Commiting...");
                            if (!string.IsNullOrEmpty(blobInfo.ValidateHash) && blobInfo.ValidateHash != hash)
                            {
                                throw new ArgumentException($"Hash in the request token is different from the uploaded file.");
                            }
                            else
                            {
                                await blobInfo.Blob.CommitBlockListAsync(blobInfo.BlockNames, commitOptions, cancellationToken: cancellationToken);
                                Logger.LogInformation($"FileId: {this.StoreName}/{blobInfo.FileId}. Commited. Elapsed time: {DateTime.Now - blobInfo.StartTime.Value}");

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
                            throw;
                        }
                        finally
                        {
                            blobInfo.Done = true;
                            blobInfo.Dispose();
                        }
                    }
                    GC.Collect();
                    return length;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"FileId: {this.StoreName}/{blobInfo.FileId}. ERROR: {ex.Message}. Elapsed time: {DateTime.Now - blobInfo.StartTime.Value}");
                throw;
            }
        }

        public async Task<string> CreateFileAsync(long uploadLength, string metadata, CancellationToken cancellationToken)
        {
            System.Security.Claims.ClaimsPrincipal? user = null;

            if (HttpContextAccessor?.HttpContext != null)
            {
                user = await Authentication.TusAuthenticationHelper.GetUser(HttpContextAccessor.HttpContext, Authentication.Constants.UPLOAD_FILE_SCHEMA);
            }
            if (user == null)
            {
                throw new ArgumentException($"Invalid upload token.");
            }
            else
            {
                Authentication.UploadProperties properties = Authentication.TusAuthenticationHelper.ParseClaims(user.Claims);

                // Get upload properties.
                var container = TusAzureHelper.GetContainer(this.BlobService, properties.Container ?? this.DefaultContainer);
                var blobName = properties.Blob;
                var useQueueAsync = properties.UseQueueAsync;
                var fileName = properties.FileName;
                string blobId;
                BlockBlobClient blob;

                if (properties.Size != uploadLength)
                {
                    // Check that requested token length is the same that the final length.
                    throw new ArgumentException($"The size in the request token ({properties.Size}) is different than the size in the uploading file ({uploadLength}).");
                }
                else if (properties.FirstRequestExpired < DateTimeOffset.UtcNow)
                {
                    // Check that the token has not expired.
                    var ex = new ArgumentException($"Request token has expired.");

                    Logger?.LogError(ex, $"Request: {properties.FirstRequestExpired}. Current: {DateTimeOffset.UtcNow}. Differnce: {properties.FirstRequestExpired - DateTimeOffset.UtcNow}");
                    throw ex;
                }
                else
                {
                    // If blobName was not set, create a new automatically.
                    if (string.IsNullOrEmpty(blobName))
                    {
                        blobName = Guid.NewGuid().ToString(); // TODO: Se podrá quitar unas versiones más tarde
                    }
                    blob = container.GetBlockBlobClient(blobName);
                    blobId = $"{blob.BlobContainerName}/{blob.Name}"; // TODO: Se podrá quitar unas versiones más tarde

                    // Check if blob already exists and it can be replaced.
                    if (await blob.ExistsAsync(cancellationToken))
                    {
                        var allowReplace = properties.Replace;

                        if (allowReplace != true)
                        {
                            throw new ArgumentException($"Blob {this.StoreName}/{blobId} already exists. Set 'replace' argument to overwrite it.");
                        }
                    }

                    // Create blob.
                    Blobs.Add(blobId, new BlobInfo(blobId, blob.BlobContainerName, blob.Name, fileName, metadata, uploadLength, useQueueAsync, blob)
                    {
                        ContentType = properties.ContentType,
                        ContentLanguage = properties.ContentLanguage,
                        ValidateHash = properties.Hash
                    });
                    return blobId;
                }
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
            long? length;

            if (Blobs.TryGetValue(fileId, out BlobInfo? blob))
            {
                length = blob.UploadLength;
            }
            else
            {
                length = null;
            }
            return Task.FromResult(length);
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
