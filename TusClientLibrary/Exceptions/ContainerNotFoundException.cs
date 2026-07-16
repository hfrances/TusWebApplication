using System;

namespace TusClientLibrary.Exceptions
{

    /// <summary>
    /// The requested blob container does not exist.
    /// </summary>
    public sealed class ContainerNotFoundException : TusHandledException
    {

        /// <summary>
        /// Gets the name of the blob storage.
        /// </summary>
        public string StorageName { get; }

        /// <summary>
        /// Gets the name of the blob container.
        /// </summary>
        public string ContainerName { get; }

        internal ContainerNotFoundException(string storageName, string containerName, Exception innerException)
            : base($"The container '{containerName}' was not found in blob storage '{storageName}'.", innerException)
        {
            StorageName = storageName;
            ContainerName = containerName;
        }

    }
}
