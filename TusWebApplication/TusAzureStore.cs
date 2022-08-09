using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Interfaces;
using tusdotnet.Models;

namespace TusWebApplication
{
    sealed class TusAzureStore : ITusStore, ITusCreationStore, ITusTerminationStore, ITusReadableStore
    {

        private class BlobInfo
        {
            public string ContainerName { get; }
            public string Metadata { get; }
            public long UploadLength { get; }
            public BlockBlobClient Blob { get; }
            public IList<string> BlockNames { get; }
                = new List<string>();
            public long SizeOffset { get; set; }

            public BlobInfo(string containerName, string metadata, long uploadLength, BlockBlobClient blob)
            {
                this.ContainerName = containerName;
                this.Metadata = metadata;
                this.UploadLength = uploadLength;
                this.Blob = blob;
            }

        }

        Azure.Storage.Blobs.BlobServiceClient BlobService { get; }
        string DefaultContainer { get; }
        Dictionary<string, BlobInfo> Blobs { get; } = new Dictionary<string, BlobInfo>();


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

        public async Task<long> AppendDataAsync(string fileId, Stream stream, CancellationToken cancellationToken)
        {

            try
            {
                var blobInfo = Blobs[fileId];
                var blockId = $"{Convert.ToBase64String(Encoding.UTF8.GetBytes(blobInfo.BlockNames.Count.ToString("d6")))}";
                long length = 0;

                using (var memo = new MemoryStream())
                {
                    await stream.CopyToAsync(memo, cancellationToken);
                    memo.Position = 0;
                    length = memo.Length;

                    _ = await blobInfo.Blob.StageBlockAsync(blockId, memo, cancellationToken: cancellationToken);
                    blobInfo.SizeOffset += length;
                    blobInfo.BlockNames.Add(blockId);
                }
                if (blobInfo.SizeOffset == blobInfo.UploadLength)
                {
                    var commitOptions = new Azure.Storage.Blobs.Models.CommitBlockListOptions()
                    {
                        Tags = new Dictionary<string, string>(),
                        Metadata = new Dictionary<string, string>()
                    };
                    var metadataParsed = tusdotnet.Parsers.MetadataParser.ParseAndValidate(MetadataParsingStrategy.AllowEmptyValues, blobInfo.Metadata).Metadata;
                    var fileName = metadataParsed["filename"].GetString(System.Text.Encoding.UTF8);
                    var container = metadataParsed["container"].GetString(System.Text.Encoding.UTF8);
                    var factor = metadataParsed["factor"].GetString(System.Text.Encoding.UTF8);

                    commitOptions.Tags.Add("filename", fileName);
                    commitOptions.Metadata.Add("factor", factor);
                    await blobInfo.Blob.CommitBlockListAsync(blobInfo.BlockNames, commitOptions, cancellationToken: cancellationToken);
                }
                return length;
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

                Blobs.Add(blobId, new BlobInfo(containerName, metadata, uploadLength, blob));
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
