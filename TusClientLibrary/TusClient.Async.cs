using qckdev.Net.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace TusClientLibrary
{
    public sealed partial class TusClient
    {

        public async Task<TusUploaderAsync> CreateFileAsync(
            FileInfo file,
            string storeName, string containerName,
            string blobName = null, bool replace = false,
            IDictionary<string, string> tags = null, IDictionary<string, string> metadata = null,
            bool useQueueAsync = false
        )
        {
            var tusClient = new TusDotNetClient.TusClient();
            UploadToken uploadToken;

            /* Authorize */
            await AuthorizeAsync();

            /* Create upload-token */
            uploadToken = await InnerHttpClient.FetchAsync<UploadToken>(HttpMethod.Post, $"files/{storeName}/{containerName}/request-upload", new
            {
                fileName = file.Name,
                blob = blobName,
                replace,
                size = file.Length,
                hash = (string)null // TODO: calculate MD5
            });
            tusClient.ApplyAuthorization(uploadToken.AccessToken);

            /* Create blob */
            string fileUrl;
            Uri uri;
            
            uri = new Uri(this.BaseAddress, $"files/{storeName}");
            fileUrl = await tusClient.CreateAsync(
                uri.OriginalString,
                file,
                TusHelper.CreateMedatada(containerName, blobName, replace, tags, metadata, useQueueAsync)
            );
            return new TusUploaderAsync(this.BaseAddress, tusClient, uploadToken, file, fileUrl);
        }

        public async Task<string> GenerateSasUrlAsync(string fileUrl, TimeSpan expiresOn)
        {
            var fileUri = new Uri(fileUrl);
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
            queryParametersSas = HttpUtility.ParseQueryString(await InnerHttpClient.FetchAsync<string>(HttpMethod.Post, requestUri.Uri.OriginalString, new
            {
                expiresOn = DateTimeOffset.Now.Add(expiresOn)
            }));
            foreach (var parameter in queryParametersSas)
            {
                queryParameters.Add(parameter.Key, parameter.Value);
            }
            result = new UriBuilder(fileUrl)
            {
                Query = HttpUtility.BuildQueryString(queryParameters)
            };
            return result.ToString();
        }


        private async Task AuthorizeAsync()
        {

            if (this.Credentials != null)
            {
                if (this.AuthorizationToken == null || AuthorizationToken.Expired < DateTimeOffset.Now)
                {
                    InnerHttpClient.DefaultRequestHeaders.Authorization = null;

                    this.AuthorizationToken = await GetTokenAsync(InnerHttpClient, Credentials.UserName, Credentials.Login, Credentials.Password);
                    InnerHttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthorizationToken.AccessToken);
                }
            }
        }

        static async Task<Token> GetTokenAsync(HttpClient client, string userName, string login, string password)
        {
            var token = await client.FetchAsync<Token>(HttpMethod.Post, "auth", new
            {
                userName,
                login,
                password
            });

            return token;
        }

    }
}
