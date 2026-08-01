
namespace TusWebApplication.AzureBlobProvider
{
    public sealed class AzureStorageCredentialSettings
    {

        public string AccountName { get; set; } = string.Empty;
        public string AccountKey { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the default container where store blob elements when it has not been defined. This parameter is optional.
        /// </summary>
        public string? DefaultContainer { get; set; }

        /// <summary>
        /// Gets or sets if it is possible to upload files to the blob asynchronously. Default value is true.
        /// </summary>
        public bool CanUploadAsync { get; set; } = true;

        /// <summary>
        /// Gets or sets if the store can only be queried and downloaded from. Default value is false.
        /// </summary>
        public bool ReadOnly { get; set; } = false;

    }
}
