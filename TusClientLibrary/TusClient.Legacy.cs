using qckdev.Net.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TusClientLibrary
{
    public sealed partial class TusClient
    {

        #region Sync

        /// <summary>
        /// Creates a blob file and returns a <see cref="TusUploader"/> object to upload its content.
        /// </summary>
        /// <param name="file"><see cref="FileInfo"/> object with the file to upload.</param>
        /// <param name="storeName">The name of the store where place the file.</param>
        /// <param name="containerName">The name of the container of the <paramref name="storeName"/>.</param>
        /// <param name="blobName">Optional. Name of the blob in the <paramref name="storeName"/>. If null, the service autogenerates one.</param>
        /// <param name="replace">Optional. If the <paramref name="blobName"/> is set and a blob with the same name already exists, it is replaced. If blob versioning is enabled, it creates a new version.</param>
        /// <param name="tags">Optional. A list of tags added to the blob. Tags can be used for filtering in the blob.</param>
        /// <param name="metadata">Optional. A list of metadatas added to the blob.</param>
        /// <param name="useQueueAsync">Optional. When it is true, the process does not wait up to the file is stored in the target. It will requires to check status in <see cref="GetFileDetails(string)"./></param>
        /// <returns>An <see cref="TusUploader"/> object to upload the content.</returns>
        public TusUploader CreateFile(
            FileInfo file,
            string storeName, string containerName,
            string blobName = null, bool replace = false,
            IDictionary<string, string> tags = null, IDictionary<string, string> metadata = null,
            bool useQueueAsync = false)
        {
            return CreateFile(storeName, containerName, file.Name, file.Length, blobName, replace, tags, metadata, useQueueAsync);
        }

        /// <summary>
        /// Creates a blob file and returns a <see cref="TusUploader"/> object to upload its content.
        /// </summary>
        /// <param name="storeName">The name of the store where place the file.</param>
        /// <param name="containerName">The name of the container of the <paramref name="storeName"/>.</param>
        /// <param name="fileName">The name of the file stored in <paramref name="storeName"/>. This is the name that the file has when it is downloaded. Please do not confuse with the <paramref name="blobName"/>.</param>
        /// <param name="fileSize">Lenght of the <paramref name="fileName"/>.</param>
        /// <param name="blobName">Optional. Name of the blob in the <paramref name="storeName"/>. If null, the service autogenerates one.</param>
        /// <param name="replace">Optional. If the <paramref name="blobName"/> is set and a blob with the same name already exists, it is replaced. If blob versioning is enabled, it creates a new version.</param>
        /// <param name="tags">Optional. A list of tags added to the blob. Tags can be used for filtering in the blob.</param>
        /// <param name="metadata">Optional. A list of metadatas added to the blob.</param>
        /// <param name="useQueueAsync">Optional. When it is true, the process does not wait up to the file is stored in the target. It will requires to check status in <see cref="GetFileDetails(string)"./></param>
        /// <returns>An <see cref="TusUploader"/> object to upload the content.</returns>
        public TusUploader CreateFile(
            string storeName, string containerName,
            string fileName, long fileSize,
            string blobName = null, bool replace = false,
            IDictionary<string, string> tags = null, IDictionary<string, string> metadata = null,
            bool useQueueAsync = false, string hash = null)
        {
            return CreateFile(storeName, containerName, fileName, fileSize, blobName, replace, new CreateFileOptions
            {
                Tags = tags,
                Metadata = metadata,
                UseQueueAsync = useQueueAsync,
                Hash = hash
            });
        }

        /// <summary>
        /// Generates a temporal token with metadata for requesting permissions to other application to upload a file.
        /// </summary>
        /// <param name="storeName">The name of the store where place the file.</param>
        /// <param name="containerName">The name of the container of the <paramref name="storeName"/>.</param>
        /// <param name="fileName">The name of the file stored in <paramref name="storeName"/>. This is the name that the file has when it is downloaded. Please do not confuse with the <paramref name="blobName"/>.</param>
        /// <param name="fileSize">Lenght of the <paramref name="fileName"/>.</param>
        /// <param name="blobName">Optional. Name of the blob in the <paramref name="storeName"/>. If null, the service autogenerates one.</param>
        /// <param name="replace">Optional. If the <paramref name="blobName"/> is set and a blob with the same name already exists, it is replaced. If blob versioning is enabled, it creates a new version.</param>
        /// <param name="useQueueAsync">Optional. When it is true, the process does not wait up to the file is stored in the target. It will requires to check status in <see cref="GetFileDetails(string)"./></param>
        /// <returns>A <see cref="UploadToken"/> with the token necessary to upload a new file.</returns>
        public UploadToken RequestUpload(
            string storeName, string containerName,
            string fileName, long fileSize,
            string blobName = null, bool replace = false,
            bool useQueueAsync = false, string hash = null)
        {
            return RequestUpload(storeName, containerName, fileName, fileSize, blobName, replace, new RequestUploadOptions
            {
                UseQueueAsync = useQueueAsync,
                Hash = hash
            });
        }

        #endregion


        #region Async

        /// <summary>
        /// Creates a blob file and returns a <see cref="TusUploaderAsync"/> object to upload its content.
        /// </summary>
        /// <param name="file"><see cref="FileInfo"/> object with the file to upload.</param>
        /// <param name="storeName">The name of the store where place the file.</param>
        /// <param name="containerName">The name of the container of the <paramref name="storeName"/>.</param>
        /// <param name="blobName">Optional. Name of the blob in the <paramref name="storeName"/>. If null, the service autogenerates one.</param>
        /// <param name="replace">Optional. If the <paramref name="blobName"/> is set and a blob with the same name already exists, it is replaced. If blob versioning is enabled, it creates a new version.</param>
        /// <param name="tags">Optional. A list of tags added to the blob. Tags can be used for filtering in the blob.</param>
        /// <param name="metadata">Optional. A list of metadatas added to the blob.</param>
        /// <param name="useQueueAsync">Optional. When it is true, the process does not wait up to the file is stored in the target. It will requires to check status in <see cref="GetFileDetailsAsync(string)"./></param>
        /// <returns>An <see cref="TusUploaderAsync"/> object to upload the content.</returns>
        public Task<TusUploaderAsync> CreateFileAsync(
            FileInfo file,
            string storeName, string containerName,
            string blobName = null, bool replace = false,
            IDictionary<string, string> tags = null, IDictionary<string, string> metadata = null,
            bool useQueueAsync = false)
        {
            return CreateFileAsync(storeName, containerName, file.Name, file.Length, blobName, replace, tags, metadata, useQueueAsync);
        }

        /// <summary>
        /// Creates a blob file and returns a <see cref="TusUploaderAsync"/> object to upload its content.
        /// </summary>
        /// <param name="storeName">The name of the store where place the file.</param>
        /// <param name="containerName">The name of the container of the <paramref name="storeName"/>.</param>
        /// <param name="fileName">The name of the file stored in <paramref name="storeName"/>. This is the name that the file has when it is downloaded. Please do not confuse with the <paramref name="blobName"/>.</param>
        /// <param name="fileSize">Lenght of the <paramref name="fileName"/>.</param>
        /// <param name="blobName">Optional. Name of the blob in the <paramref name="storeName"/>. If null, the service autogenerates one.</param>
        /// <param name="replace">Optional. If the <paramref name="blobName"/> is set and a blob with the same name already exists, it is replaced. If blob versioning is enabled, it creates a new version.</param>
        /// <param name="tags">Optional. A list of tags added to the blob. Tags can be used for filtering in the blob.</param>
        /// <param name="metadata">Optional. A list of metadatas added to the blob.</param>
        /// <param name="useQueueAsync">Optional. When it is true, the process does not wait up to the file is stored in the target. It will requires to check status in <see cref="GetFileDetailsAsync(string)"./></param>
        /// <returns>An <see cref="TusUploaderAsync"/> object to upload the content.</returns>
        public Task<TusUploaderAsync> CreateFileAsync(
            string storeName, string containerName,
            string fileName, long fileSize,
            string blobName = null, bool replace = false,
            IDictionary<string, string> tags = null, IDictionary<string, string> metadata = null,
            bool useQueueAsync = false, string hash = null)
        {
            return CreateFileAsync(storeName, containerName, fileName, fileSize, blobName, replace, new CreateFileOptions
            {
                Tags = tags,
                Metadata = metadata,
                UseQueueAsync = useQueueAsync,
                Hash = hash
            });
        }

        /// <summary>
        /// Generates a temporal token with metadata for requesting permissions to other application to upload a file.
        /// </summary>
        /// <param name="storeName">The name of the store where place the file.</param>
        /// <param name="containerName">The name of the container of the <paramref name="storeName"/>.</param>
        /// <param name="fileName">The name of the file stored in <paramref name="storeName"/>. This is the name that the file has when it is downloaded. Please do not confuse with the <paramref name="blobName"/>.</param>
        /// <param name="fileSize">Lenght of the <paramref name="fileName"/>.</param>
        /// <param name="blobName">Optional. Name of the blob in the <paramref name="storeName"/>. If null, the service autogenerates one.</param>
        /// <param name="replace">Optional. If the <paramref name="blobName"/> is set and a blob with the same name already exists, it is replaced. If blob versioning is enabled, it creates a new version.</param>
        /// <param name="useQueueAsync">Optional. When it is true, the process does not wait up to the file is stored in the target. It will requires to check status in <see cref="GetFileDetails(string)"./></param>
        /// <returns>A <see cref="UploadToken"/> with the token necessary to upload a new file.</returns>
        public Task<UploadToken> RequestUploadAsync(
            string storeName, string containerName,
            string fileName, long fileSize,
            string blobName = null, bool replace = false,
            bool useQueueAsync = false, string hash = null)
        {
            return RequestUploadAsync(storeName, containerName, fileName, fileSize, blobName, replace, new RequestUploadOptions
            {
                UseQueueAsync = useQueueAsync,
                Hash = hash
            });
        }

        #endregion

    }
}
