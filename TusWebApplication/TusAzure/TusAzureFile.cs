using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using tusdotnet.Interfaces;
using tusdotnet.Models;

namespace TusWebApplication.TusAzure
{
    public sealed class TusAzureFile : ITusFile
    {

        BlobClient BlobClient { get; }
        BlobProperties? BlobProperties { get; }

        public TusAzureFile(BlobClient blob)
        {
            this.BlobClient = blob;

            this.Exists = blob.Exists();
            if (this.Exists)
            {
                this.BlobProperties = BlobClient.GetProperties();
            }
        }

        public string Id => $"{BlobClient.BlobContainerName}/{BlobClient.Name}";

        public bool Exists { get; }

        public Task<Stream> GetContentAsync(CancellationToken cancellationToken)
        {
            return BlobClient.OpenReadAsync();
        }

        public async Task<Dictionary<string, Metadata>> GetMetadataAsync(CancellationToken cancellationToken)
        {
            var metadatastrings = new List<string>();

            if (BlobProperties != null)
            {
                var metadata = BlobProperties.Metadata;

                foreach (var meta in metadata)
                {
                    string k = meta.Key.Replace(" ", "").Replace(",", "");
                    string v = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(meta.Value));
                    metadatastrings.Add(string.Format("{0} {1}", k, v));
                }
            }
            if (this.Exists)
            {
                var tags = await BlobClient.GetTagsAsync();

                foreach (var tag in tags.Value.Tags)
                {
                    string k = tag.Key.Replace(" ", "").Replace(",", "");
                    string v = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(tag.Value));
                    metadatastrings.Add(string.Format("{0} {1}", k, v));
                }
                metadatastrings.Add($"container {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(BlobClient.BlobContainerName))}");
            }
            return tusdotnet.Parsers.MetadataParser.ParseAndValidate(MetadataParsingStrategy.AllowEmptyValues, string.Join(",", metadatastrings.ToArray())).Metadata;
        }
    }
}
