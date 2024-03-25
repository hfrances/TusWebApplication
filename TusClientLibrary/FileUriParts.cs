using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TusClientLibrary
{
    public sealed class FileUriParts
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

        public static FileUriParts Parse(string fileUrl)
        {
            var fileUri = new Uri(fileUrl, UriKind.RelativeOrAbsolute);

            return Parse(fileUri);
        }

        public static FileUriParts Parse(Uri fileUri)
        {
            FileUriParts result;
            string[] split;
            int index;
            string versionId;
            Uri basePath;

            if (fileUri.IsAbsoluteUri)
            {
                // Absolute Uri: ignore "authority" and take the rest.
                split = fileUri.AbsolutePath.Split('/');
            }
            else
            {
                // Relative Uri: cannot use any property excepting the "original string".
                split = fileUri.OriginalString.Split('/'); // Ri
            }
            // Find where is the "files" uri part.
            index = Array.FindIndex(split, x => string.Equals(x, UriHelper.FILES_PATH, StringComparison.OrdinalIgnoreCase));
            // Extract versionId.
            UriHelper.ExtractParametersFromUri(
                new Uri(new Uri("http://localhost"), fileUri),
                out versionId, out _
            );
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
