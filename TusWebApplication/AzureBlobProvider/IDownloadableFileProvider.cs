using Microsoft.Extensions.FileProviders;

namespace TusWebApplication.AzureBlobProvider
{

    /// <summary>
    /// A read-only file provider abstraction.
    /// </summary>
    public interface IDownloadableFileProvider : IFileProvider
    {

        /// <summary>
        /// Locate a file at the given path.
        /// </summary>
        /// <param name="subpath">Relative path that identifies the file.</param>
        /// <returns>The file information. Caller must check Exists property.</returns>
        new IDownloadableFileInfo GetFileInfo(string subpath);

        IFileInfo IFileProvider.GetFileInfo(string subpath)
            => GetFileInfo(subpath);

    }
}
