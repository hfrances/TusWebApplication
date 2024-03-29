﻿using Azure.Storage.Blobs.Specialized;
using System;
using System.IO;
using System.Linq;

namespace TusWebApplication.AzureBlobProvider
{

    sealed class AzureBlobFileInfo : IDownloadableFileInfo
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
                var metadata = properties.Metadata.ToDictionary(x => x.Key, x => Uri.UnescapeDataString(x.Value), StringComparer.OrdinalIgnoreCase);
                this.Name = metadata.FirstOrDefault(x => x.Key.Equals("filename", StringComparison.OrdinalIgnoreCase)).Value ?? blob.Name;
                this.ContentType = properties.ContentType ?? "application/octet-stream";
                this.LastModified = properties.LastModified;
            }
            else
            {
                this.Name = blob.Name;
            }
        }

        public string Name { get; }

        public bool IsDirectory => false;

        public string ContentType { get; }

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
