using System;

namespace TusClientLibrary.Exceptions
{

    /// <summary>
    /// The requested blob storage is not configured in the service.
    /// </summary>
    public sealed class BlobStorageNotFoundException : TusHandledException
    {

        /// <summary>
        /// Gets the name of the blob storage.
        /// </summary>
        public string StorageName { get; }

        internal BlobStorageNotFoundException(string storageName, Exception innerException)
            : base($"The blob storage '{storageName}' is not configured.", innerException)
        {
            StorageName = storageName;
        }

    }
}
