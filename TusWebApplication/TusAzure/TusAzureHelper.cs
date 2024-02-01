﻿using System.Collections.Generic;
using System;
using tusdotnet.Models;
using System.Threading.Tasks;
using tusdotnet.Interfaces;
using System.Threading;

namespace TusWebApplication.TusAzure
{

    static class TusAzureHelper
    {

        public static Azure.Storage.Blobs.BlobServiceClient CreateBlobServiceClient(string accountName, string accountKey)
        {
            var credentials = new Azure.Storage.StorageSharedKeyCredential(accountName, accountKey);
            var blobUri = new Uri($"https://{accountName}.blob.core.windows.net");

            return new Azure.Storage.Blobs.BlobServiceClient(blobUri, credentials);
        }

        public static Azure.Storage.Blobs.BlobContainerClient GetContainer(Azure.Storage.Blobs.BlobServiceClient client, string containerName)
        {
            var container = client.GetBlobContainerClient(containerName);

            if (container == null)
            {
                throw new Exception($"Container with name '{containerName}' not found.");
            }
            else
            {
                return container;
            }
        }

        public static Task<ITusFile> GetFileAsync(Azure.Storage.Blobs.BlobServiceClient client, string fileId, CancellationToken cancellationToken)
        {
            var blobId = fileId.Split('/');

            if (blobId.Length == 2)
            {
                var containerName = blobId[0];
                var blobName = blobId[1];
                var container = client.GetBlobContainerClient(containerName);

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

        public static Azure.Storage.Blobs.Models.CommitBlockListOptions CreateCommitBlockListOptions(BlobInfo blobInfo)
        {
            var commitOptions = new Azure.Storage.Blobs.Models.CommitBlockListOptions()
            {
                Tags = new Dictionary<string, string>(),
                Metadata = new Dictionary<string, string>(),
                HttpHeaders = new Azure.Storage.Blobs.Models.BlobHttpHeaders()
                {
                    ContentHash = blobInfo.Hasher.Hash
                }
            };
            var metadataParsed = tusdotnet.Parsers.MetadataParser.ParseAndValidate(MetadataParsingStrategy.AllowEmptyValues, blobInfo.Metadata).Metadata;
            var fileNameFixed = Uri.EscapeDataString(blobInfo.FileName);

            foreach (var (key, value) in metadataParsed)
            {
                var stringValue = value.GetString(System.Text.Encoding.UTF8);
                var stringValueFixed = Uri.EscapeDataString(stringValue);

                if (key.StartsWith("BLOB:", StringComparison.OrdinalIgnoreCase))
                {
                    // Do Nothing.
                }
                else if (key.StartsWith("TAG:", StringComparison.OrdinalIgnoreCase))
                {
                    commitOptions.Tags.Add(key[4..], stringValueFixed);
                }
                else
                {
                    commitOptions.Metadata.Add(key, stringValueFixed);
                }
            }

            commitOptions.Metadata["filename"] = fileNameFixed;
            return commitOptions;
        }

    }

}
