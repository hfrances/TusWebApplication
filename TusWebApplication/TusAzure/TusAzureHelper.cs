using System.Collections.Generic;
using System;
using tusdotnet.Models;
using System.Linq;
using System.Threading.Tasks;
using tusdotnet.Interfaces;
using System.Threading;

namespace TusWebApplication.TusAzure
{

    static class TusAzureHelper
    {

        public static Azure.Storage.Blobs.BlobContainerClient GetContainer(Azure.Storage.Blobs.BlobServiceClient client, string metadata, string defaultContainer)
        {
            var metadataParsed = tusdotnet.Parsers.MetadataParser.ParseAndValidate(MetadataParsingStrategy.AllowEmptyValues, metadata)?.Metadata;
            Metadata? containerMetadata;
            string containerName;

            containerMetadata = metadataParsed?.SingleOrDefault(x => x.Key.Equals("container", StringComparison.OrdinalIgnoreCase)).Value;
            if (containerMetadata == null)
            {
                containerName = defaultContainer;
            }
            else
            {
                containerName = containerMetadata.GetString(System.Text.Encoding.UTF8);
            }

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

            foreach (var (key, value) in metadataParsed)
            {
                var stringValue = value.GetString(System.Text.Encoding.UTF8);

                if (key.Equals("container", StringComparison.OrdinalIgnoreCase))
                {
                    // Do Nothing.
                }
                else if (key.StartsWith("META:", StringComparison.OrdinalIgnoreCase))
                {
                    commitOptions.Metadata.Add(key[5..], stringValue);
                }
                else
                {
                    commitOptions.Tags.Add(key, stringValue);
                }
            }
            return commitOptions;
        }

    }

}
