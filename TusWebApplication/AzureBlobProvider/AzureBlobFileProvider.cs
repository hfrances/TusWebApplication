﻿using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;

namespace TusWebApplication.AzureBlobProvider
{
    public sealed class AzureBlobFileProvider : IDownloadableFileProvider
    {

        AzureStorageCredentialsSettings AzureSettings { get; }

        public AzureBlobFileProvider(IOptions<AzureStorageCredentialsSettings> azureOptions)
        {
            this.AzureSettings = azureOptions.Value;
        }

        public IDownloadableFileInfo GetFileInfo(string subpath)
        {
            var url = new Uri(new Uri("http://localhost"), subpath);
            var blobId = url.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var storageName = blobId.First();

            if (AzureSettings.TryGetValue(storageName, out AzureStorageCredentialSettings? settings))
            {
                var blobService = AzureBlobHelper.CreateBlobServiceClient(
                    settings.AccountName,settings.AccountKey
                );

                if (blobId.Length == 3)
                {
                    var containerName = blobId[1];
                    var blobName = blobId[2];
                    var container = blobService.GetBlobContainerClient(containerName);

                    if (container.Exists())
                    {
                        var query = System.Web.HttpUtility.ParseQueryString(url.Query);
                        var versionId = query.Get("versionId");
                        var blob = container.GetBlockBlobClient(blobName);

                        if (!string.IsNullOrWhiteSpace(versionId))
                        {
                            blob = blob.WithVersion(versionId);
                        }
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
