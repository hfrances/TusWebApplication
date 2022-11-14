using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Linq;

namespace TusWebApplication.AzureBlobProvider
{

    sealed class AzureBlobFileInfo : IFileInfo
    {
        long _length;

        BlockBlobClient Blob { get; }

        public bool Exists { get; }

        public AzureBlobFileInfo(BlockBlobClient blob)
        {
            this.Blob = blob;
            this.Exists = blob.Exists();

            if (this.Exists)
            {
                var properties = blob.GetProperties().Value;

                this.Name = properties.Metadata.FirstOrDefault(x => x.Key.Equals("filename", StringComparison.OrdinalIgnoreCase)).Value ?? blob.Name;
                this.LastModified = properties.LastModified;
            }
            else
            {
                this.Name = blob.Name;
            }
        }

        public string Name { get; }

        public bool IsDirectory => false;

        public DateTimeOffset LastModified { get; }

        public long Length
        {
            get
            {
                if (this.Exists)
                {
                    return _length;
                }
                else
                {
                    throw new FileNotFoundException($"File not found: {Blob.BlobContainerName}/{Blob.Name}");
                }
            }
        }

        public string? PhysicalPath => null;

        public Stream CreateReadStream()
            => Blob.OpenRead();

    }

}
