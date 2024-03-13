using qckdev.Net.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using TusDotNetClient;

namespace TusClientLibrary
{

    public sealed partial class TusClient
    {
        public delegate void ProgressedDelegate(long transferred, long total);

        HttpClient InnerHttpClient { get; }
        TusClientCredentials Credentials { get; }
        Token AuthorizationToken { get; set; }

        public Uri BaseAddress { get; }

        public TusClient(Uri baseAddress)
        {
            this.BaseAddress = baseAddress;
            this.InnerHttpClient = new HttpClient() { BaseAddress = baseAddress };
        }

        public TusClient(Uri baseAddress, TusClientCredentials credentials)
            : this(baseAddress)
        {
            this.Credentials = credentials;
        }


        /// <summary>
        /// Creates a blob file and returns a <see cref="TusUploader"/> object to upload its content.
        /// </summary>
        /// <param name="file"><see cref="FileInfo"/> object with the file to upload.</param>
        /// <param name="storeName">The name of the store where place the file.</param>
        /// <param name="containerName">The name of the container of the <paramref name="storeName"/>.</param>
        /// <param name="blobName">Optional. Name of the blob in the <paramref name="storeName"/>. If null, the service autogenerates one.</param>
        /// <param name="replace">Optional. If the <paramref name="blobName"/> is set and a blob with the same name already exists, it is replaced. If blob versioning is enabled, it creates a new version.</param>
        /// <param name="options">Optional. Additional options for the file.</param>
        /// <returns>An <see cref="TusUploader"/> object to upload the content.</returns>
        public TusUploader CreateFile(
            FileInfo file,
            string storeName, string containerName,
            string blobName = null, bool replace = false,
            CreateFileOptions options = null)
        {
            return CreateFile(storeName, containerName, file.Name, file.Length, blobName, replace, options);
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
        /// <param name="options">Optional. Additional options for the file.</param>
        /// <returns>An <see cref="TusUploader"/> object to upload the content.</returns>
        public TusUploader CreateFile(
            string storeName, string containerName,
            string fileName, long fileSize,
            string blobName = null, bool replace = false,
            CreateFileOptions options = null)
        {
            var tusClient = new TusDotNetClient.TusClient();
            UploadToken uploadToken;

            /* Authorize */
            Authorize();

            /* Create upload-token */
            uploadToken = RequestUpload(storeName, containerName, fileName, fileSize, blobName, replace, options);
            tusClient.ApplyAuthorization(uploadToken.AccessToken);

            /* Create blob */
            string fileUrl;
            Uri uri;

            try
            {
                uri = new Uri(this.BaseAddress, $"files/{storeName}");
                fileUrl = tusClient.CreateAsync(
                    uri.OriginalString,
                    fileSize,
                    TusHelper.CreateMedatada(options?.Tags, options?.Metadata)
                ).Result;
                return new TusUploader(this.BaseAddress, tusClient, uploadToken, fileUrl);
            }
            catch (AggregateException ex) when (ex.InnerException is TusDotNetClient.TusException tusex)
            {
                var response = TusHelper.ParseResponse(tusex.ResponseContent);

                throw new Exception(response?.Error?.Message ?? tusex.Message, tusex);
            }
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
        /// <param name="options">Optional. Additional options for the file.</param>
        /// <returns>A <see cref="UploadToken"/> with the token necessary to upload a new file.</returns>
        public UploadToken RequestUpload(
            string storeName, string containerName,
            string fileName, long fileSize,
            string blobName = null, bool replace = false,
            RequestUploadOptions options = null)
        {
            UploadToken uploadToken;

            /* Authorize */
            Authorize();

            /* Create upload-token */
            uploadToken = InnerHttpClient.Fetch<UploadToken>(HttpMethod.Post, $"files/{storeName}/{containerName}/request-upload", new
            {
                fileName,
                blob = blobName,
                replace,
                size = fileSize,
                contentType = options?.ContentType,
                contentLanguage = options?.ContentLanguage,
                options?.Hash,
                options?.UseQueueAsync
            });
            return uploadToken;
        }

        /// <summary>
        /// Returns information about an specific blob.
        /// </summary>
        /// <param name="fileUrl">The file url. Url can contains the file version (https://..../container/blobname?versionId=xxxxxxx).</param>
        /// <param name="includeVersions">Optional. Sets if must load all versions. It can increase response time.</param>
        /// <returns>A <see cref="FileDetails"/> with the information about the blob.</returns>
        public FileDetails GetFileDetails(string fileUrl, bool includeVersions = false)
        {
            var fileUri = new Uri(fileUrl);
            UriBuilder requestUri;
            IDictionary<string, string> queryParameters;
            string versionId;

            // Extract versionId from the url and pass to the overloaded method.
            queryParameters = HttpUtility.ParseQueryString(fileUri.Query);
            if (queryParameters.TryGetValue("versionId", out versionId))
            {
                queryParameters.Remove("versionId");
            }
            // Build Uri.
            requestUri = new UriBuilder(fileUri.GetLeftPart(UriPartial.Path))
            {
                Query = HttpUtility.BuildQueryString(queryParameters)
            };
            return GetFileDetails(requestUri.Uri.ToString(), versionId, includeVersions);
        }

        /// <summary>
        /// Returns information about an specific blob.
        /// </summary>
        /// <param name="fileUrl">The file url. Url can contains the file version (https://..../container/blobname?versionId=xxxxxxx).</param>
        /// <param name="includeVersions">Optional. Sets if must load all versions. It can increase response time.</param>
        /// <returns>A <see cref="FileDetails"/> with the information about the blob.</returns>
        public FileDetails GetFileDetails(string fileUrl, string versionId, bool includeVersions = false)
        {
            FileDetails result;
            var fileUri = new Uri(fileUrl);
            var queryParameters = HttpUtility.ParseQueryString(fileUri.Query);
            UriBuilder requestUri;

            // Replaces "versionId" for the specified in the parameter (if it is in the fileUrl, it will be replaced or removed).
            if (versionId != null)
            {
                queryParameters["versionId"] = versionId;
            }
            queryParameters["loadVersions"] = includeVersions.ToString();
            requestUri = new UriBuilder($"{fileUri.GetLeftPart(UriPartial.Path)}/details")
            {
                Query = HttpUtility.BuildQueryString(queryParameters)
            };

            // Request.
            Authorize();
            result = InnerHttpClient.Fetch<FileDetails>(HttpMethod.Get, requestUri.Uri.ToString());
            return result;
        }

        /// <summary>
        /// Returns an url that includes a temporal shared access signature.
        /// </summary>
        /// <param name="fileUrl">The original url.</param>
        /// <param name="expiresOn">The time during which the URL will be available.</param>
        /// <returns>An url that includes a temparl shared access signarute.</returns>
        public string GenerateSasUrl(string fileUrl, TimeSpan expiresOn)
            => GenerateSasUrl(new Uri(fileUrl), expiresOn).ToString();

        /// <summary>
        /// Returns an url that includes a temporal shared access signature.
        /// </summary>
        /// <param name="fileUrl">The original url.</param>
        /// <param name="expiresOn">The time during which the URL will be available.</param>
        /// <returns>An url that includes a temparl shared access signarute.</returns>
        public Uri GenerateSasUrl(Uri fileUri, TimeSpan expiresOn)
        {
            UriBuilder requestUri;
            UriBuilder result;
            IDictionary<string, string> queryParameters, queryParametersSas;

            Authorize();
            requestUri = new UriBuilder($"{fileUri.GetLeftPart(UriPartial.Path)}/generateSas")
            {
                Query = fileUri.Query
            };

            // Get URL queries, original and SAS token and merge them for the result.
            queryParameters = HttpUtility.ParseQueryString(fileUri.Query);
            queryParametersSas = HttpUtility.ParseQueryString(InnerHttpClient.Fetch<string>(HttpMethod.Post, requestUri.Uri.ToString(), new
            {
                expiresOn = DateTimeOffset.UtcNow.Add(expiresOn)
            }));
            foreach (var parameter in queryParametersSas)
            {
                queryParameters[parameter.Key] = parameter.Value;
            }
            result = new UriBuilder(fileUri)
            {
                Query = HttpUtility.BuildQueryString(queryParameters)
            };
            return result.Uri;
        }

        /// <summary>
        /// Uploads the specified file using a request upload token.
        /// </summary>
        /// <param name="fileUrl">Url of the file to upload. Use <seealso cref="RequestUpload"/> to get one.</param>
        /// <param name="requestToken">Request token of the file to upload. Use <seealso cref="RequestUpload"/> to get one.</param>
        /// <param name="fileInfo"><see cref="System.IO.FileInfo"/> of the file to upload.</param>
        /// <param name="chunkSize">Size (in MB) of the chunks to send.</param>
        /// <param name="progressed">Callback to report upload progress.</param>
        public static void UploadFile(
            string fileUrl, string requestToken,
            FileInfo fileInfo,
            double chunkSize = 5D,
            ProgressedDelegate progressed = null)
        {
            TusUploader.Upload(fileUrl, requestToken, fileInfo, chunkSize, progressed);
        }

        /// <summary>
        /// Uploads the specified file using a request upload token.
        /// </summary>
        /// <param name="fileUrl">Url of the file to upload. Use <seealso cref="RequestUpload"/> to get one.</param>
        /// <param name="requestToken">Request token of the file to upload. Use <seealso cref="RequestUpload"/> to get one.</param>
        /// <param name="fileStream"><see cref="System.IO.Stream"/> of the file to upload.</param>
        /// <param name="chunkSize">Size (in MB) of the chunks to send.</param>
        /// <param name="progressed">Callback to report upload progress.</param>
        public static void UploadFile(
            string fileUrl, string requestToken,
            Stream fileStream,
            double chunkSize = 5D,
            ProgressedDelegate progressed = null)
        {
            TusUploader.Upload(fileUrl, requestToken, fileStream, chunkSize, progressed);
        }

        private void Authorize()
        {

            if (this.Credentials != null)
            {
                if (this.AuthorizationToken == null || AuthorizationToken.Expired < DateTimeOffset.Now)
                {
                    InnerHttpClient.DefaultRequestHeaders.Authorization = null;

                    this.AuthorizationToken = GetToken(InnerHttpClient, Credentials.UserName, Credentials.Login, Credentials.Password);
                    InnerHttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthorizationToken.AccessToken);
                }
            }
        }

        static Token GetToken(HttpClient client, string userName, string login, string password)
        {
            var token = client.Fetch<Token>(HttpMethod.Post, "auth", new
            {
                userName,
                login,
                password
            });

            return token;
        }

    }
}
