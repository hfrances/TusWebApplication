using System;

namespace TusClientLibrary.Exceptions
{

    /// <summary>
    /// The requested blob storage is configured as read-only.
    /// </summary>
    public sealed class ReadOnlyStoreException : TusHandledException
    {

        /// <summary>
        /// Gets the name of the blob storage.
        /// </summary>
        public string StorageName { get; }

        internal ReadOnlyStoreException(string storageName, Exception innerException)
            : base($"The blob storage '{storageName}' is read-only.", innerException)
        {
            StorageName = storageName;
        }

    }
}
