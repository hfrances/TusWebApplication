using qckdev.Net.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;

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

        
        public TusUploader CreateFile(
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
            Authorize();

            /* Create upload-token */
            uploadToken = InnerHttpClient.Fetch<UploadToken>(HttpMethod.Post, $"files/{storeName}/{containerName}/request-upload", new
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
            fileUrl = tusClient.CreateAsync(
                uri.OriginalString, 
                file, 
                TusHelper.CreateMedatada(containerName, blobName, replace, tags, metadata, useQueueAsync)
            ).Result;
            return new TusUploader(this.BaseAddress, tusClient, uploadToken, file, fileUrl);
        }

        public string GenerateSasUrl(string fileUrl, TimeSpan expiresOn)
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
            queryParametersSas = HttpUtility.ParseQueryString(InnerHttpClient.Fetch<string>(HttpMethod.Post, requestUri.Uri.ToString(), new
            {
                expiresOn = DateTimeOffset.UtcNow.Add(expiresOn)
            }));
            foreach (var parameter in queryParametersSas)
            {
                queryParameters.Add(parameter.Key, parameter.Value);
            }
            result = new UriBuilder(new Uri(fileUrl))
            {
                Query = HttpUtility.BuildQueryString(queryParameters)
            };
            return result.Uri.ToString();
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
