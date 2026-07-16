using System;

namespace TusClientLibrary.Exceptions
{

    /// <summary>
    /// The requested blob does not exist.
    /// </summary>
    public sealed class BlobNotFoundException : TusHandledException
    {

        /// <summary>
        /// Gets the name of the blob storage.
        /// </summary>
        public string StorageName { get; }

        /// <summary>
        /// Gets the name of the blob container.
        /// </summary>
        public string ContainerName { get; }

        /// <summary>
        /// Gets the name of the blob.
        /// </summary>
        public string BlobName { get; }

        internal BlobNotFoundException(string storageName, string containerName, string blobName, Exception innerException)
            : base($"The blob '{blobName}' was not found in container '{containerName}' of blob storage '{storageName}'.", innerException)
        {
            StorageName = storageName;
            ContainerName = containerName;
            BlobName = blobName;
        }

    }
}
