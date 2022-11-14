using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;

namespace TusWebApplication.AzureBlobProvider
{
    public sealed class AzureBlobFileProvider : IFileProvider
    {

        Azure.Storage.Blobs.BlobServiceClient BlobService { get; }

        public AzureBlobFileProvider(IOptions<AzureStorageCredentialSettings> options)
        {
            this.BlobService = AzureBlobHelper.CreateBlobServiceClient(
                options.Value.AccountName ?? string.Empty,
                options.Value.AccountKey ?? string.Empty
            );
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var blobId = subpath.Split('/');

            if (blobId.Length == 2)
            {
                var containerName = blobId[0];
                var blobName = blobId[1];
                var container = BlobService.GetBlobContainerClient(containerName);

                if (container.Exists())
                {
                    var blob = container.GetBlockBlobClient(blobName);

                    return new AzureBlobFileInfo(blob);
                }
                else
                {
                    throw new ArgumentException($"Container with name {containerName} does not exist.");
                }
            }
            else
            {
                throw new ArgumentException("Path must contain {container}/{blob}");
            }
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            throw new System.NotImplementedException();
        }

        public IChangeToken Watch(string filter)
        {
            throw new System.NotImplementedException();
        }
    }
}
