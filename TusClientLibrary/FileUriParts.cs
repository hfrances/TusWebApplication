using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TusClientLibrary
{
    sealed class FileUriParts
    {

        public Uri BasePath { get; set; }
        public string StoreName { get; set; }
        public string ContainerName { get; set; }
        public string BlobName { get; set; }
        public string BlobId => $"{ContainerName}/{BlobName}";
        public string VersionId { get; set; }


        /// <summary>
        /// Extracts all elements from a file <see cref="Uri"/>
        /// </summary>
        /// <param name="basePath">The working base path.</param>
        /// <param name="fileUri">Blob file <see cref="Uri"/></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static FileUriParts Parse(Uri basePath, Uri fileUri)
        {
            FileUriParts result;
            var fileUriFixed = UriHelper.ExtractParametersFromUri(fileUri, out string versionId, out _);
            var relativeUri = basePath.MakeRelativeUri(fileUriFixed);
            var split = relativeUri.ToString().Split('/');

            if (split.Length >= 4 && split.First().Equals(UriHelper.FILES_PATH, StringComparison.OrdinalIgnoreCase))
            {
                result = new FileUriParts
                {
                    BasePath = basePath,
                    StoreName = split[1],
                    ContainerName = split[2],
                    BlobName = string.Join("/", split.Skip(3)),
                    VersionId = versionId,
                };
            }
            else
            {
                throw new FormatException($"Invalid url format for relative uri '{relativeUri}'.");
            }
            return result;
        }

    }
}
