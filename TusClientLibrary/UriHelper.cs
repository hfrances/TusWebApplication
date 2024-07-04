using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TusClientLibrary
{

    static class UriHelper
    {

        /// <summary>
        /// The subpath name of the files controller.
        /// </summary>
        public const string FILES_PATH = "files";


        /// <summary>
        /// Returns a path for "file" controller.
        /// </summary>
        /// <param name="subpaths">Sub paths of the "file" url. Example: file/subpath1/subpath2.</param>
        public static string GetRelativeFileUrl(params string[] subpaths)
            => string.Join("/", Enumerable.Union(new[] { FILES_PATH }, (subpaths ?? new string[] { })).ToArray());

        /// <summary>
        /// Returns a path with the version and the includeVersions query parameters.
        /// If the <paramref name="fileUri"/> already contains those parameters, they are replaced (excepting if they are null).
        /// </summary>
        /// <param name="fileUri">The current file uri where add the query parameters.</param>
        /// <param name="versionId">Sets the "version" query parameter. Null for ignore it.</param>
        /// <param name="includeVersions">Sets the "includeVersion" query parameter. Null for ignore it.</param>
        public static Uri GetBlobUriWithVersion(Uri fileUri, string versionId, bool? includeVersions = null)
            => GetBlobUriWithVersion(fileUri, null, versionId, includeVersions);

        /// <summary>
        /// Returns a path with the version and the includeVersions query parameters.
        /// If the <paramref name="fileUrl"/> already contains those parameters, they are replaced (excepting if they are null).
        /// </summary>
        /// <param name="fileUrl">The current file uri where add the query parameters.</param>
        /// <param name="versionId">Sets the "version" query parameter. Null for ignore it.</param>
        /// <param name="includeVersions">Sets the "includeVersion" query parameter. Null for ignore it.</param>
        public static string GetBlobUriWithVersion(string fileUrl, string versionId, bool? includeVersions = null)
        {
            var baseUri = new Uri("http://localhost");
            var fileUri = new Uri(baseUri, fileUrl);
            Uri outputUri;

            outputUri = GetBlobUriWithVersion(fileUri, versionId, includeVersions);
            return baseUri.MakeRelativeUri(outputUri).ToString();
        }

        /// <summary>
        /// Returns a path with the version and the includeVersions query parameters.
        /// If the <paramref name="fileUri"/> already contains those parameters, they are replaced (excepting if they are null).
        /// </summary>
        /// <param name="fileUri">The current file uri where add the query parameters.</param>
        /// <param name="subpath">Sub path of the "file" url. Example: file/subpath1</param>
        /// <param name="versionId">Sets the "version" query parameter. Null for ignore it.</param>
        /// <param name="includeVersions">Sets the "includeVersion" query parameter. Null for ignore it.</param>
        public static Uri GetBlobUriWithVersion(Uri fileUri, string subpath, string versionId, bool? includeVersions = null)
        {
            var queryParameters = HttpHelper.ParseQueryString(fileUri.Query);
            UriBuilder requestUri;

            // Replaces "versionId" for the specified in the parameter (if it is in the fileUrl, it will be replaced or removed).
            if (versionId != null)
            {
                queryParameters["versionId"] = versionId;
            }
            if (includeVersions != null)
            {
                queryParameters["loadVersions"] = includeVersions.ToString();
            }

            // Build uri.
            requestUri = new UriBuilder($"{fileUri.GetLeftPart(UriPartial.Path)}")
            {
                Query = HttpHelper.BuildQueryString(queryParameters)
            };
            if (!string.IsNullOrEmpty(subpath?.TrimEnd()))
            {
                requestUri.Path += $"/{subpath}";
            }
            return requestUri.Uri;
        }

        /// <summary>
        /// Returns a <see cref="Uri"/> removing "version" and "includeVersions" query parameters. 
        /// Those parameters are extracted in out parameters <paramref name="versionId"/> and <paramref name="includeVersions"/>.
        /// </summary>
        /// <param name="fileUrl">The current file uri where extract the query parameters.</param>
        /// <param name="versionId">Output parameter with the "version" query value, or null if it is not present.</param>
        /// <param name="includeVersions">Output parameter with the "includeVersions" query value, or null if it is not present.</param>
        /// <returns>The original <see cref="Uri"/> without "version" and "includeVersions" query parameters.</returns>
        public static string ExtractParametersFromUri(string fileUrl, out string versionId, out bool? includeVersions)
        {
            var baseUri = new Uri("http://localhost");
            var fileUri = new Uri(baseUri, fileUrl);
            Uri outputUri;

            outputUri = ExtractParametersFromUri(fileUri, out versionId, out includeVersions);
            return baseUri.MakeRelativeUri(outputUri).ToString();
        }


        /// <summary>
        /// Returns a <see cref="Uri"/> removing "version" and "includeVersions" query parameters. 
        /// Those parameters are extracted in out parameters <paramref name="versionId"/> and <paramref name="includeVersions"/>.
        /// </summary>
        /// <param name="fileUri">The current file uri where extract the query parameters.</param>
        /// <param name="versionId">Output parameter with the "version" query value, or null if it is not present.</param>
        /// <param name="includeVersions">Output parameter with the "includeVersions" query value, or null if it is not present.</param>
        /// <returns>The original <see cref="Uri"/> without "version" and "includeVersions" query parameters.</returns>
        public static Uri ExtractParametersFromUri(Uri fileUri, out string versionId, out bool? includeVersions)
            => ExtractParametersFromUri(fileUri, null, out versionId, out includeVersions);

        /// <summary>
        /// Returns a <see cref="Uri"/> removing "version" and "includeVersions" query parameters. 
        /// Those parameters are extracted in out parameters <paramref name="versionId"/> and <paramref name="includeVersions"/>.
        /// </summary>
        /// <param name="fileUri">The current file uri where extract the query parameters.</param>
        /// <param name="subpath">Sub path of the "file" url. Example: file/subpath1</param>
        /// <param name="versionId">Output parameter with the "version" query value, or null if it is not present.</param>
        /// <param name="includeVersions">Output parameter with the "includeVersions" query value, or null if it is not present.</param>
        /// <returns>The original <see cref="Uri"/> without "version" and "includeVersions" query parameters.</returns>
        public static Uri ExtractParametersFromUri(Uri fileUri, string subpath, out string versionId, out bool? includeVersions)
        {
            UriBuilder requestUri;
            IDictionary<string, string> queryParameters;

            // Extract versionId from the url and pass to the overloaded method.
            queryParameters = HttpHelper.ParseQueryString(fileUri.Query);
            if (queryParameters.TryGetValue("versionId", out versionId))
            {
                queryParameters.Remove("versionId");
            }

            // Extract includeVersions from the url and pass to the overloaded method.
            includeVersions = null;
            if (queryParameters.TryGetValue("includeVersions", out string includeVersionsString))
            {
                queryParameters.Remove("includeVersions");
                if (bool.TryParse(includeVersionsString, out bool includeVersionsBool))
                {
                    includeVersions = includeVersionsBool;
                }
            }

            // Build Uri.
            requestUri = new UriBuilder(fileUri.GetLeftPart(UriPartial.Path))
            {
                Query = HttpHelper.BuildQueryString(queryParameters)
            };
            if (!string.IsNullOrEmpty(subpath?.TrimEnd()))
            {
                requestUri.Path += $"/{subpath}";
            }
            return requestUri.Uri;
        }


    }

}
