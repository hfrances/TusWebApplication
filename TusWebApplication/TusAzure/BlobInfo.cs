using Azure.Storage.Blobs.Specialized;
using System.Collections.Generic;
using System.Security.Cryptography;
using System;

namespace TusWebApplication.TusAzure
{
    sealed class BlobInfo : IDisposable
    {

        /// <summary>
        /// Primary key of the blob in the process.
        /// </summary>
        public string FileId { get; }
        /// <summary>
        /// Container name.
        /// </summary>
        public string ContainerName { get; }
        /// <summary>
        /// Blob name.
        /// </summary>
        public string BlobName { get; set; }
        /// <summary>
        /// File name of the blob.
        /// </summary>
        public string? FileName { get; set; }
        /// <summary>
        /// Metadata to upload into the blob.
        /// </summary>
        public string Metadata { get; }
        /// <summary>
        /// Size of the file (in bytes).
        /// </summary>
        public long UploadLength { get; }
        /// <summary>
        /// Represents the blob to upload.
        /// </summary>
        public BlockBlobClient? Blob { get; private set; }
        /// <summary>
        /// A list with the chunk names that is going to upload to the blob storage.
        /// </summary>
        public IList<string> BlockNames { get; }
            = new List<string>();
        /// <summary>
        /// Chunks uploaded from the client. Chunks already uploaded are removed from this list.
        /// </summary>
        public IList<QueueItem> Queue { get; }
            = new List<QueueItem>();
        /// <summary>
        /// If true, chunks are stored in a pool and the process takes from this pool for uploading to blob storage.
        /// Blob storage uploading velocitiy does not affect to client upload velocity.
        /// </summary>
        public bool UseQueueAsync { get; }
        /// <summary>
        /// Number of chunks uploaded from the client.
        /// </summary>
        public int QueueCount { get; set; }
        /// <summary>
        /// Number of chunks uploaded to the blob storage.
        /// </summary>
        public int QueuePosition { get; set; }
        /// <summary>
        /// Size (in bytes) uploaded from the client.
        /// </summary>
        public long SizeOffset { get; set; }
        /// <summary>
        /// Size (in bytes) uploaded to the blob storage.
        /// </summary>
        public long SizeOffsetInternal { get; set; }
        /// <summary>
        /// Time when the process started.
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// Hash algorithm for calculating MD5 value. 
        /// </summary>
        public HashAlgorithm Hasher { get; }

        /// <summary>
        /// Gets or sets if the process is already finished.
        /// </summary>
        public bool Done { get; set; }
        /// <summary>
        /// Gets or sets if the process had some error.
        /// </summary>
        public Exception? Error { get; set; }


        public BlobInfo(string fileId, string containerName, string blobName, string? fileName, string metadata, long uploadLength, bool useQueueAsync, BlockBlobClient blob)
        {
            FileId = fileId;
            ContainerName = containerName;
            BlobName = blobName;
            FileName = fileName;
            Metadata = metadata;
            UploadLength = uploadLength;
            UseQueueAsync = useQueueAsync;
            Blob = blob;

            Hasher = MD5.Create();
            Hasher.Initialize();
        }

        public void Dispose()
        {
            foreach (var blob in Queue)
            {
                blob.Dispose();
            };
            Queue.Clear();
            BlockNames.Clear();
            Hasher.Dispose();
            Blob = null;
        }
    }
}
