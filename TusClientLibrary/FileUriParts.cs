using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TusClientLibrary
{

    /// <summary>
    /// Provides an object representation of a file uniform resource identifier (URI) and easy access to the parts of it.
    /// </summary>
    public sealed class FileUriParts
    {

        /// <summary>
        /// Gets or sets the working base path.
        /// </summary>
        public Uri BasePath { get; set; }

        /// <summary>
        /// Gets or sets the name of the store where the file is placed.
        /// </summary>
        public string StoreName { get; set; }

        /// <summary>
        /// Gets or sets the name of the container of the <see cref="StoreName"/>.
        /// </summary>
        public string ContainerName { get; set; }

        /// <summary>
        /// Gets or sets name of the blob in the <see cref="StoreName"/>.
        /// </summary>
        public string BlobName { get; set; }

        /// <summary>
        /// Gets the concatenation of <see cref="ContainerName"/> and <see cref="BlobName"/>.
        /// </summary>
        public string BlobId => $"{ContainerName}/{BlobName}";

        /// <summary>
        /// Gets or sets the version id of the blob or null if it was not set.
        /// </summary>
        public string VersionId { get; set; }


        /// <summary>
        /// Gets the file relative url including <see cref="VersionId"/> if it was defined.
        /// </summary>
        /// <param name="withVersion">True for including <see cref="VersionId"/> (if it was defined). False for excluding it.</param>
        public string GetRelativeUrl(bool withVersion = true)
        {
            string result;

            result = UriHelper.GetRelativeFileUrl(this.StoreName, this.BlobId);
            if (withVersion)
            {
                result = UriHelper.GetBlobUriWithVersion(result, this.VersionId);
            }
            return result;
        }


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
            var fileUriFixed = new Uri(basePath, fileUri);
            var fileUriClean = UriHelper.ExtractParametersFromUri(fileUriFixed, out string versionId, out _);
            var relativeUri = basePath.MakeRelativeUri(fileUriClean);
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

        /// <summary>
        /// Extracts all elements from a file <see cref="Uri"/>
        /// </summary>
        /// <param name="fileUrl">Blob file.</param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static FileUriParts Parse(string fileUrl)
        {
            var fileUri = new Uri(fileUrl, UriKind.RelativeOrAbsolute);

            return Parse(fileUri);
        }

        /// <summary>
        /// Extracts all elements from a file <see cref="Uri"/>
        /// </summary>
        /// <param name="fileUri">Blob file <see cref="Uri"/></param>
        /// <returns></returns>
        /// <exception cref="FormatException"></exception>
        public static FileUriParts Parse(Uri fileUri)
        {
            FileUriParts result;
            string[] split;
            int index;
            string versionId;
            Uri basePath;

            if (fileUri.IsAbsoluteUri)
            {
                Uri absoluteUrl;

                // Absolute Uri: ignore "authority" and take the rest.
                absoluteUrl = UriHelper.ExtractParametersFromUri(fileUri, out versionId, out _);
                split = absoluteUrl.AbsolutePath.Split('/');
            }
            else
            {
                string relativeUrl;

                // Relative Uri: cannot use any property excepting the "original string".
                relativeUrl = UriHelper.ExtractParametersFromUri(fileUri.OriginalString, out versionId, out _);
                split = relativeUrl.Split('/');
            }
            // Find where is the "files" uri part.
            index = Array.FindIndex(split, x => string.Equals(x, UriHelper.FILES_PATH, StringComparison.OrdinalIgnoreCase));
            // Uri must contains almost 
            if (index >= 0 && split.Skip(index).Count() >= 4)
            {
                if (fileUri.IsAbsoluteUri)
                {
                    // Build the absolute uri taking the authority and the previous elements to the "files" uri part.
                    basePath = new Uri(new Uri(fileUri.GetLeftPart(UriPartial.Authority)), string.Join("/", split.Take(index)));
                }
                else
                {
                    basePath = null;
                }
                result = new FileUriParts
                {
                    BasePath = basePath,
                    StoreName = split[index + 1],
                    ContainerName = split[index + 2],
                    BlobName = string.Join("/", split.Skip(index + 3)),
                    VersionId = versionId,
                };
            }
            else
            {
                throw new FormatException($"Invalid url format for relative uri '{fileUri}'.");
            }
            return result;
        }

    }
}
