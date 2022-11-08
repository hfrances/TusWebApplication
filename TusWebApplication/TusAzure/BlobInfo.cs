using Azure.Storage.Blobs.Specialized;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;

namespace TusWebApplication.TusAzure
{
    sealed class BlobInfo
    {
        public string FileId { get; }
        public string ContainerName { get; }
        public string BlobName { get; set; }
        public string Metadata { get; }
        public long UploadLength { get; }
        public BlockBlobClient Blob { get; }
        public IList<string> BlockNames { get; }
            = new List<string>();
        public IList<QueueItem> Queue { get; }
            = new List<QueueItem>();
        public long SizeOffset { get; set; }
        public long SizeOffsetInternal { get; set; }
        public DateTime? StartTime { get; set; }
        public HashAlgorithm Hasher { get; }

        public BlobInfo(string fileId, string containerName, string blobName, string metadata, long uploadLength, BlockBlobClient blob)
        {
            FileId = fileId;
            ContainerName = containerName;
            BlobName = blobName;
            Metadata = metadata;
            UploadLength = uploadLength;
            Blob = blob;

            Hasher = MD5.Create();
            Hasher.Initialize();
        }

    }
}
