using Azure.Core;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;

namespace TusWebApplication.AzureBlobProvider
{
    public sealed class AzureBlobFileProvider : IFileProvider
    {

        AzureStorageCredentialsSettings AzureSettings { get; }

        public AzureBlobFileProvider(IOptions<AzureStorageCredentialsSettings> azureOptions)
        {
            this.AzureSettings = azureOptions.Value;
        }

        public IFileInfo GetFileInfo(string subpath)
        {
            var blobId = subpath.Split('/');
            var storageName = blobId.First();

            if (AzureSettings.TryGetValue(storageName, out AzureStorageCredentialSettings? settings))
            {
                var blobService = AzureBlobHelper.CreateBlobServiceClient(
                    settings.AccountName ?? string.Empty,
                    settings.AccountKey ?? string.Empty
                );

                if (blobId.Length == 3)
                {
                    var containerName = blobId[1];
                    var blobName = blobId[2];
                    var container = blobService.GetBlobContainerClient(containerName);

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
            else
            {
                throw new ArgumentException($"Invalid storage name: '{storageName}'.");
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
