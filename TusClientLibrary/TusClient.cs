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

        TusDotNetClient.TusClient InnerTusClient { get; }
        HttpClient InnerHttpClient { get; }
        TusClientCredentials Credentials { get; }
        Token AuthorizationToken { get; set; }

        public Uri BaseAddress { get; }

        public TusClient(Uri baseAddress)
        {
            this.BaseAddress = baseAddress;
            this.InnerTusClient = new TusDotNetClient.TusClient();
            this.InnerHttpClient = new HttpClient() { BaseAddress = baseAddress };
        }

        public TusClient(Uri baseAddress, TusClientCredentials credentials)
            : this(baseAddress)
        {
            this.Credentials = credentials;
        }


        public string CreateFile(
            FileInfo file,
            string storeName, string containerName,
            string blobName = null, bool replace = false,
            IDictionary<string, string> tags = null, IDictionary<string, string> metadata = null,
            bool useQueueAsync = false
        )
        {
            /* Authorize */
            Authorize();

            /* Create blob */
            string fileUrl;
            Uri uri;
            var metadataParsed = new List<(string key, string value)>
            {
                // properties exclusively for upload process.
                ("BLOB:container", containerName), // target container.
                ("BLOB:name", blobName ?? string.Empty), // blob storage name.
                ("BLOB:replace", replace.ToString()), // if exists, replace it (requires BLOB:name).
                ("BLOB:useQueueAsync", useQueueAsync.ToString()), // if true, after upload from client to service, it does not wait for uplodad from service to blob storage.
            };

            if (tags != null)
            {
                // tags
                foreach (var item in tags)
                {
                    metadataParsed.Add(($"TAG:{item.Key}", item.Value));
                }
            }
            if (metadata != null)
            {
                // metadata
                foreach (var item in metadata)
                {
                    metadataParsed.Add((item.Key, item.Value));
                }
            }

            uri = new Uri(this.BaseAddress, $"files/{storeName}");
            fileUrl = InnerTusClient.CreateAsync(uri.OriginalString, file, metadataParsed.ToArray()).Result;
            return fileUrl;
        }

        public void Upload(
            string fileUrl, FileInfo file, double chunkSize = 5D,
            ProgressedDelegate progressed = null)
        {
            var uploadOperation = InnerTusClient.UploadAsync(fileUrl, file, chunkSize);

            if (progressed != null)
            {
                uploadOperation.Progressed += (transferred, total) =>
                {
                    progressed(transferred, total);
                };
            }
            uploadOperation.Operation.Wait();
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
                expiresOn = DateTimeOffset.Now.Add(expiresOn)
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
                    if (InnerTusClient.AdditionalHeaders.ContainsKey("Authorization"))
                    {
                        InnerTusClient.AdditionalHeaders.Remove("Authorization");
                    }
                    InnerHttpClient.DefaultRequestHeaders.Authorization = null;

                    this.AuthorizationToken = GetToken(InnerHttpClient, Credentials.UserName, Credentials.Login, Credentials.Password);
                    InnerTusClient.AdditionalHeaders.Add("Authorization", $"Bearer {AuthorizationToken.AccessToken}");
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
