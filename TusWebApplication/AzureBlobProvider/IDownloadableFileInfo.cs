namespace TusWebApplication.AzureBlobProvider
{
    public interface IDownloadableFileInfo : Microsoft.Extensions.FileProviders.IFileInfo
    {

        /// <summary>
        /// The MIME content type of the file.
        /// </summary>
        public string ContentType { get; }

    }
}
