﻿#if NO_ASYNC
#else
using TusDotNetClient = qckdev.Storage.TusDotNetClient;
using qckdev.Net.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace TusClientLibrary
{
    public sealed partial class TusClient
    {

        /// <summary>
        /// Creates a blob file and returns a <see cref="TusUploaderAsync"/> object to upload its content.
        /// </summary>
        /// <param name="file"><see cref="FileInfo"/> object with the file to upload.</param>
        /// <param name="storeName">The name of the store where place the file.</param>
        /// <param name="containerName">The name of the container of the <paramref name="storeName"/>.</param>
        /// <param name="blobName">Optional. Name of the blob in the <paramref name="storeName"/>. If null, the service autogenerates one.</param>
        /// <param name="replace">Optional. If the <paramref name="blobName"/> is set and a blob with the same name already exists, it is replaced. If blob versioning is enabled, it creates a new version.</param>
        /// <param name="options">Optional. Additional options for the file.</param>
        /// <returns>An <see cref="TusUploaderAsync"/> object to upload the content.</returns>
        public Task<TusUploaderAsync> CreateFileAsync(
            FileInfo file,
            string storeName, string containerName,
            string blobName = null, bool replace = false,
            CreateFileOptions options = null)
        {
            return CreateFileAsync(storeName, containerName, file.Name, file.Length, blobName, replace, options);
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
        /// <param name="options">Optional. Additional options for the file.</param>
        /// <returns>An <see cref="TusUploaderAsync"/> object to upload the content.</returns>
        public async Task<TusUploaderAsync> CreateFileAsync(
            string storeName, string containerName,
            string fileName, long fileSize,
            string blobName = null, bool replace = false,
            CreateFileOptions options = null)
        {
            var tusClient = new TusDotNetClient.TusClient();
            UploadToken uploadToken;

            /* Authorize */
            await AuthorizeAsync();

            /* Create upload-token */
            uploadToken = await RequestUploadAsync(storeName, containerName, fileName, fileSize, blobName, replace, options);
            tusClient.ApplyAuthorization(uploadToken.AccessToken);

            /* Create blob */
            string fileUrl;
            Uri uri;

            try
            {
                uri = new Uri(this.BaseAddress, UriHelper.GetRelativeFileUrl(storeName));
                fileUrl = await tusClient.CreateAsync(
                    uri.OriginalString,
                    fileSize,
                    TusHelperAsync.CreateMedatada(options?.Tags, options?.Metadata)
                );
                return new TusUploaderAsync(this.BaseAddress, tusClient, uploadToken, fileUrl);
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
        public async Task<UploadToken> RequestUploadAsync(
            string storeName, string containerName,
            string fileName, long fileSize,
            string blobName = null, bool replace = false,
            RequestUploadOptions options = null)
        {
            UploadToken uploadToken;
            string path;

            try
            {
                /* Authorize */
                await AuthorizeAsync();

                /* Create upload-token */
                path = UriHelper.GetRelativeFileUrl(storeName, containerName, "request-upload");
                uploadToken = await HttpHelper.CreateHttpWebRequest(
                        HttpRequestMethod.Post, this.BaseAddress, path, new
                        {
                            fileName,
                            blob = blobName,
                            replace,
                            size = fileSize,
                            contentType = options?.ContentType,
                            contentTypeAuto = options?.ContentTypeAuto,
                            contentLanguage = options?.ContentLanguage,
                            hash = options?.Hash,
                            useQueueAsync = options?.UseQueueAsync ?? false
                        },
                        AuthorizationToken.AccessToken
                    ).FetchAsync<UploadToken, TusResponse>();
                return uploadToken;
            }
            catch (FetchFailedException<TusResponse> ex)
            {
                throw new Exception(ex.Error?.Error?.Message ?? ex.Message, ex);
            }
        }

        /// <summary>
        /// Returns information about an specific blob.
        /// </summary>
        /// <param name="storeName">The name of the store where the file is placed.</param>
        /// <param name="containerName">The name of the container of the <paramref name="storeName"/>.</param>
        /// <param name="blobName">Name of the blob in the <paramref name="storeName"/>.</param>
        /// <param name="includeVersions">Optional. Sets if must load all versions. It can increase response time.</param>
        /// <returns>A <see cref="FileDetails"/> with the information about the blob.</returns>
        public Task<FileDetails> GetFileDetailsAsync(string storeName, string containerName, string blobName, bool includeVersions = false)
            => GetFileDetailsAsync(UriHelper.GetRelativeFileUrl(storeName, containerName, blobName), includeVersions);

        /// <summary>
        /// Returns information about an specific blob.
        /// </summary>
        /// <param name="fileUrl">The file url. Url can contains the file version (https://..../container/blobname?versionId=xxxxxxx).</param>
        /// <param name="includeVersions">Optional. Sets if must load all versions. It can increase response time.</param>
        /// <returns>A <see cref="FileDetails"/> with the information about the blob.</returns>
        public Task<FileDetails> GetFileDetailsAsync(string fileUrl, bool includeVersions = false)
        {
            var fileUri = new Uri(this.BaseAddress, fileUrl);
            Uri requestUri;
            string versionId;

            requestUri = UriHelper.ExtractParametersFromUri(fileUri, out versionId, out _);
            return GetFileDetailsAsync(requestUri.ToString(), versionId, includeVersions);
        }

        /// <summary>
        /// Returns information about an specific blob.
        /// </summary>
        /// <param name="storeName">The name of the store where the file is placed.</param>
        /// <param name="containerName">The name of the container of the <paramref name="storeName"/>.</param>
        /// <param name="blobName">Name of the blob in the <paramref name="storeName"/>.</param>
        /// <param name="includeVersions">Optional. Sets if must load all versions. It can increase response time.</param>
        /// <returns>A <see cref="FileDetails"/> with the information about the blob.</returns>
        public Task<FileDetails> GetFileDetailsAsync(string storeName, string containerName, string blobName, string versionId, bool includeVersions = false)
            => GetFileDetailsAsync(UriHelper.GetRelativeFileUrl(storeName, containerName, blobName), versionId, includeVersions);

        /// <summary>
        /// Returns information about an specific blob.
        /// </summary>
        /// <param name="fileUrl">The file url. Url can contains the file version (https://..../container/blobname?versionId=xxxxxxx).</param>
        /// <param name="includeVersions">Optional. Sets if must load all versions. It can increase response time.</param>
        /// <returns>A <see cref="FileDetails"/> with the information about the blob.</returns>
        public async Task<FileDetails> GetFileDetailsAsync(string fileUrl, string versionId, bool includeVersions = false)
        {
            FileDetails result;
            var fileUri = new Uri(this.BaseAddress, fileUrl);
            Uri requestUri = UriHelper.GetBlobUriWithVersion(fileUri, "details", versionId, includeVersions);

            // Request.
            await AuthorizeAsync();
            result = await HttpHelper.CreateHttpWebRequest(
                    HttpRequestMethod.Get, this.BaseAddress, requestUri.ToString(),
                    tokenBearer: AuthorizationToken.AccessToken
                ).FetchAsync<FileDetails>();
            return result;
        }

        /// <summary>
        /// Returns an url that includes a temporal shared access signature.
        /// </summary>
        /// <param name="fileUrl">The original url.</param>
        /// <param name="expiresOn">The time during which the URL will be available.</param>
        /// <returns>An url that includes a temparl shared access signarute.</returns>
        public async Task<string> GenerateSasUrlAsync(string fileUrl, TimeSpan expiresOn)
            => (await GenerateSasUrlAsync(new Uri(this.BaseAddress, fileUrl), expiresOn)).ToString();

        /// <summary>
        /// Returns an url that includes a temporal shared access signature.
        /// </summary>
        /// <param name="fileUri">The original url.</param>
        /// <param name="expiresOn">The time during which the URL will be available.</param>
        /// <returns>An url that includes a temparl shared access signarute.</returns>
        public async Task<Uri> GenerateSasUrlAsync(Uri fileUri, TimeSpan expiresOn)
        {
            UriBuilder requestUri;
            UriBuilder result;
            IDictionary<string, string> queryParameters, queryParametersSas;
            string tokenSas;

            /* Authorize. */
            await AuthorizeAsync();

            /* Actions */
            requestUri = new UriBuilder($"{fileUri.GetLeftPart(UriPartial.Path)}/sas")
            {
                Query = fileUri.Query
            };

            // Get URL queries, original and SAS token and merge them for the result.
            queryParameters = HttpHelper.ParseQueryString(fileUri.Query);
            tokenSas = await HttpHelper.CreateHttpWebRequest(
                    HttpRequestMethod.Post, this.BaseAddress, requestUri.Uri.OriginalString, new
                    {
                        expiresOn = DateTimeOffset.Now.Add(expiresOn)
                    },
                    AuthorizationToken.AccessToken
                ).FetchAsync<string>();
            queryParametersSas = HttpHelper.ParseQueryString(tokenSas);
            foreach (var parameter in queryParametersSas)
            {
                queryParameters[parameter.Key] = parameter.Value;
            }
            result = new UriBuilder(fileUri)
            {
                Query = HttpHelper.BuildQueryString(queryParameters)
            };
            return result.Uri;
        }

        /// <summary>
        /// Returns an url that includes a temporal shared access signature.
        /// </summary>
        /// <param name="fileUriList">A list of urls.</param>
        /// <param name="expiresOn">The time during which the URL will be available.</param>
        /// <returns>An url that includes a temparl shared access signarute.</returns>
        public async Task<IEnumerable<TokenSas>> GenerateSasUrlAsync(IEnumerable<string> fileUriList, TimeSpan expiresOn)
        {
            var dictionary = new Dictionary<(string storeName, string containerName), IEnumerable<Uri>>();
            var fileParts = fileUriList
                .Select(x => new { OriginalUrl = x, Parts = FileUriParts.Parse(this.BaseAddress, new Uri(this.BaseAddress, x)) })
                .GroupBy(g => new { g.Parts.StoreName, g.Parts.ContainerName }); // Group paths by store and container.
            var tokenSasList = new List<TokenSasPrivate>();

            // Authorize.
            await AuthorizeAsync();

            // Generate tokens for each container and store.
            foreach (var group in fileParts)
            {
                var response
                    = await HttpHelper.CreateHttpWebRequest(
                        HttpRequestMethod.Post, this.BaseAddress, UriHelper.GetRelativeFileUrl(group.Key.StoreName, group.Key.ContainerName, "sas"),
                        new
                        {
                            expiresOn = DateTimeOffset.Now.Add(expiresOn),
                            blobs = group.Select(x => new { x.Parts.BlobName, x.Parts.VersionId })
                        },
                        AuthorizationToken.AccessToken
                    ).FetchAsync<IEnumerable<TokenSasPrivate>>();
                tokenSasList.AddRange(response);
            }

            // Return paths with token Sas.
            return tokenSasList.Select(x =>
            {
                UriBuilder uriBuilder = null;

                if (!string.IsNullOrWhiteSpace(x.TokenSas))
                {
                    var relativeUrl = UriHelper.GetRelativeFileUrl(x.StoreName, x.ContainerName, x.BlobName);
                    var query = new Dictionary<string, string>();
                    var querySas = HttpHelper.ParseQueryString(x.TokenSas);

                    if (!string.IsNullOrWhiteSpace(x.Version))
                    {
                        query.Add("versionId", x.Version);
                    }
                    query = query.Union(querySas).ToDictionary(y => y.Key, y => y.Value);
                    uriBuilder = new UriBuilder(new Uri(this.BaseAddress, relativeUrl))
                    {
                        Query = HttpHelper.BuildQueryString(query)
                    };
                }
                return new TokenSas
                {
                    Url = uriBuilder?.Uri.ToString(),
                    RelativeUrl = (uriBuilder != null ? BaseAddress.MakeRelativeUri(uriBuilder.Uri).ToString() : null)
                };
            });
        }

        /// <summary>
        /// Uploads the specified file using a request upload token.
        /// </summary>
        /// <param name="fileUrl">Url of the file to upload. Use <seealso cref="RequestUpload"/> to get one.</param>
        /// <param name="requestToken">Request token of the file to upload. Use <seealso cref="RequestUpload"/> to get one.</param>
        /// <param name="fileInfo"><see cref="System.IO.FileInfo"/> of the file to upload.</param>
        /// <param name="chunkSize">Size (in MB) of the chunks to send.</param>
        /// <param name="progressed">Callback to report upload progress.</param>
        public static Task UploadFileAsync(
            string fileUrl, string requestToken,
            FileInfo fileInfo,
            double chunkSize = 5D,
            ProgressedDelegate progressed = null)
        {
            return TusUploaderAsync.UploadAsync(fileUrl, requestToken, fileInfo, chunkSize, progressed);
        }

        /// <summary>
        /// Uploads the specified file using a request upload token.
        /// </summary>
        /// <param name="fileUrl">Url of the file to upload. Use <seealso cref="RequestUpload"/> to get one.</param>
        /// <param name="requestToken">Request token of the file to upload. Use <seealso cref="RequestUpload"/> to get one.</param>
        /// <param name="fileStream"><see cref="System.IO.Stream"/> of the file to upload.</param>
        /// <param name="chunkSize">Size (in MB) of the chunks to send.</param>
        /// <param name="progressed">Callback to report upload progress.</param>
        public static Task UploadFileAsync(
            string fileUrl, string requestToken,
            Stream fileStream,
            double chunkSize = 5D,
            ProgressedDelegate progressed = null)
        {
            return TusUploaderAsync.UploadAsync(fileUrl, requestToken, fileStream, chunkSize, progressed);
        }

        /// <summary>
        /// Takes an external blob file and imports it in the specific container.
        /// For more information, see <see href="https://docs.microsoft.com/en-us/rest/api/storageservices/copy-blob">Copy Blob</see>.
        /// </summary>
        /// <param name="sourceUrl">
        /// Specifies the <see cref="Uri"/> of the source blob.  The value may
        /// be a <see cref="Uri" /> of up to 2 KB in length that specifies a
        /// blob.  A source blob in the same storage account can be
        /// authenticated via Shared Key.  However, if the source is a blob in
        /// another account, the source blob must either be public or must be
        /// authenticated via a shared access signature. If the source blob
        /// is public, no authentication is required to perform the copy
        /// operation.
        ///
        /// The source object may be a file in the Azure File service.  If the
        /// source object is a file that is to be copied to a blob, then the
        /// source file must be authenticated using a shared access signature,
        /// whether it resides in the same account or in a different account.
        /// </param>
        /// <param name="storeName">The name of the store where place the file.</param>
        /// <param name="containerName">The name of the container of the <paramref name="storeName"/>.</param>
        /// <param name="fileName">The name of the file stored in <paramref name="storeName"/>. This is the name that the file has when it is downloaded. Please do not confuse with the <paramref name="blobName"/>.</param>
        /// <param name="blobName">Optional. Name of the blob in the <paramref name="storeName"/>. If null, the service autogenerates one.</param>
        /// <param name="options">Optional. Additional import options.</param>
        /// <param name="waitForComplete">Optional. When true, this function waits until copy has been finished.</param>
        public async Task<ImportDetails> ImportFileAsync(string sourceUrl, string storeName, string containerName, string fileName, string blobName = null, UploadFileOptions options = null, bool waitForComplete = true)
        {
            ImportDetailsPrivate result;
            string path;
            Uri fileUri;

            try
            {
                /* Authorize */
                await AuthorizeAsync();

                /* Action */
                path = UriHelper.GetRelativeFileUrl(storeName, containerName, "import");
                result = await HttpHelper.CreateHttpWebRequest(
                        HttpRequestMethod.Post, this.BaseAddress, path, new
                        {
                            sourceUrl,
                            fileName,
                            targetBlobName = blobName,
                            contentType = options?.ContentType,
                            tags = options?.Tags,
                            metadata = options?.Metadata,
                            waitForComplete
                        },
                        AuthorizationToken.AccessToken
                    ).FetchAsync<ImportDetailsPrivate, TusResponse>();
                fileUri = new Uri(this.BaseAddress, UriHelper.GetRelativeFileUrl(result.StoreName, result.BlobId));
                return new ImportDetails
                {
                    Url = fileUri.ToString(),
                    RelativeUrl = BaseAddress.MakeRelativeUri(fileUri).ToString()
                };
            }
            catch (FetchFailedException<TusResponse> ex)
            {
                throw new Exception(ex.Error?.Error?.Message ?? ex.Message, ex);
            }
        }

        /// <summary>
        /// Deletes the specific blob specific blob.
        /// </summary>
        /// <param name="storeName">The name of the store where the file is placed.</param>
        /// <param name="containerName">The name of the container of the <paramref name="storeName"/>.</param>
        /// <param name="blobName">Name of the blob in the <paramref name="storeName"/>.</param>
        public Task DeleteBlobAsync(string storeName, string containerName, string blobName, string versionId = null)
            => DeleteBlobAsync(UriHelper.GetRelativeFileUrl(storeName, containerName, blobName), versionId);

        /// <summary>
        /// Deletes the specific blob specific blob.
        /// </summary>
        /// <param name="fileUrl">The file url. Url can contains the file version (https://..../container/blobname?versionId=xxxxxxx).</param>
        public async Task DeleteBlobAsync(string fileUrl, string versionId = null)
        {
            try
            {
                var fileUri = new Uri(this.BaseAddress, fileUrl);
                var requestUri = UriHelper.GetBlobUriWithVersion(fileUri, versionId);

                // Request.
                await AuthorizeAsync();
                await HttpHelper.CreateHttpWebRequest(HttpRequestMethod.Delete, this.BaseAddress, requestUri.ToString())
                    .FetchAsync<object, TusResponse>();
            }
            catch (FetchFailedException<TusResponse> ex)
            {
                throw new Exception(ex.Error?.Error?.Message ?? ex.Message, ex);
            }
        }

        /// <summary>
        /// Creates or updates the <see cref="AuthorizationToken"/> with the authorization JWT token bearer.
        /// </summary>
        /// <remarks>
        /// Checks if <see cref="AuthorizationToken"/> is exprired before regenerate it.
        /// </remarks>
        async Task AuthorizeAsync()
        {

            if (this.Credentials != null)
            {
                if (this.AuthorizationToken == null || AuthorizationToken.Expired < DateTimeOffset.Now)
                {
                    this.AuthorizationToken = await GetTokenAsync(this.BaseAddress, Credentials.UserName, Credentials.Login, Credentials.Password);
                }
            }
        }

        /// <summary>
        /// Generates a new authorization JWT token bearer.
        /// </summary>
        /// <param name="baseAddress">The base address of the TUS service.</param>
        /// <param name="userName">Authentication user name.</param>
        /// <param name="login">Authentication login.</param>
        /// <param name="password">Authentication password.</param>
        /// <returns>An authentication JWT token bearer.</returns>
        static async Task<Token> GetTokenAsync(Uri baseAddress, string userName, string login, string password)
        {
            var token = await HttpHelper.CreateHttpWebRequest(HttpRequestMethod.Post, baseAddress, "auth", new
            {
                userName,
                login,
                password
            }).FetchAsync<Token>();

            return token;
        }

    }
}
#endif