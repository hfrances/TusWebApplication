using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;

namespace TusWebApplication.AzureBlobProvider
{
    public sealed class AzureBlobFileProvider : IDownloadableFileProvider
    {

        AzureStorageCredentialsSettings AzureSettings { get; }

        public AzureBlobFileProvider(IOptions<AzureStorageCredentialsSettings> azureOptions)
        {
            this.AzureSettings = azureOptions.Value;
        }

        /// <summary>
        /// Locate a file at the given path.
        /// </summary>
        /// <param name="subpath">Relative path that identifies the file.</param>
        /// <returns>The file information. Caller must check Exists property.</returns>
        public IDownloadableFileInfo GetFileInfo(string subpath)
        {
            var blobPath = AzureBlobFileProvider.SplitUriPath(subpath);
            var blobQuery = AzureBlobFileProvider.GetUriQuery(subpath);
            var storageName = blobPath.First();

            if (AzureSettings.TryGetValue(storageName, out AzureStorageCredentialSettings? settings))
            {
                var blobService = AzureBlobHelper.CreateBlobServiceClient(
                    settings.AccountName,settings.AccountKey
                );

                if (blobPath.Length == 3)
                {
                    var containerName = blobPath[1];
                    var blobName = blobPath[2];
                    var container = blobService.GetBlobContainerClient(containerName);

                    if (container.Exists())
                    {
                        var query = System.Web.HttpUtility.ParseQueryString(blobQuery);
                        var versionId = query.Get("versionId");
                        var blob = container.GetBlockBlobClient(blobName);

                        if (!string.IsNullOrWhiteSpace(versionId))
                        {
                            blob = blob.WithVersion(versionId);
                        }
                        return new AzureBlobFileInfo(blob);
                    }
                    else
                    {
                        throw new ArgumentException($"A container with name {containerName} does not exist.");
                    }
                }
                else
                {
                    throw new ArgumentException("Path must contain {container}/{blob}");
                }
            }
            else
            {
                throw new ArgumentException($"Invalid storage name: '{storageName}'.");
            }
        }

        /// <summary>
        /// Enumerate a directory at the given path, if any.
        /// </summary>
        /// <param name="subpath">Relative path that identifies the directory.</param>
        /// <returns>The file information. Caller must check Exists property.</returns>
        /// <exception cref="NotImplementedException">
        /// This method has not been implemented because it is not necessary in this scope.
        /// </exception>
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a <see cref="Microsoft.Extensions.Primitives.IChangeToken"/> for the specified filter.
        /// </summary>
        /// <param name="filter">
        /// Filter string used to determine what files or folders to monitor. 
        /// Example: **/*.cs, *.*, subFolder/**/*.cshtml.
        /// </param>
        /// <returns>Returns the contents of the directory.</returns>
        /// <exception cref="NotImplementedException">
        /// This method has not been implemented because it is not necessary in this scope.
        /// </exception>
        public IChangeToken Watch(string filter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets an array with the <paramref name="relativeUri"/> split by character '/'. Removes empty entries.
        /// </summary>
        /// <param name="relativeUri"></param>
        static string[] SplitUriPath(string relativeUri)
        {
            var url = new Uri(new Uri("nourl://notarealhost"), relativeUri);
            return url.AbsolutePath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Gets the query information included in the <paramref name="relativeUri"/>.
        /// </summary>
        /// <param name="relativeUri"></param>
        static string GetUriQuery(string relativeUri)
        {
            var url = new Uri(new Uri("nourl://notarealhost"), relativeUri);
            return url.Query;
        }
    }
}
