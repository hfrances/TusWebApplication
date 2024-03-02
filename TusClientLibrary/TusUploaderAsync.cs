using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TusDotNetClient;
using static TusClientLibrary.TusClient;

namespace TusClientLibrary
{
    public sealed class TusUploaderAsync
    {

        TusDotNetClient.TusClient InnerTusClient { get; }
        HttpClient InnerHttpClient { get; }

        public Uri BaseAddress { get; }
        public string FileUrl { get; }
        public string RelativeUrl => new Uri(FileUrl).AbsolutePath;
        public UploadToken UploadToken { get; }

        internal TusUploaderAsync(Uri baseAddress, TusDotNetClient.TusClient tusClient, UploadToken uploadToken, string fileUrl)
        {
            this.InnerTusClient = tusClient;
            this.BaseAddress = baseAddress;
            this.UploadToken = uploadToken;
            this.FileUrl = fileUrl;

            this.InnerHttpClient = new HttpClient() { BaseAddress = baseAddress };
        }

        /// <summary>
        /// Uploads the specified file using a request upload token.
        /// </summary>
        /// <param name="fileInfo"><see cref="System.IO.FileInfo"/> of the file to upload.</param>
        /// <param name="chunkSize">Size (in MB) of the chunks to send.</param>
        /// <param name="progressed">Callback to report upload progress.</param>
        public Task UploadAsync(
            FileInfo fileInfo,
            double chunkSize = 5D,
            ProgressedDelegate progressed = null)
        {
            var uploadOperation = InnerTusClient.UploadAsync(this.FileUrl, fileInfo, chunkSize);

            return PerformUploadAsync(uploadOperation, progressed);
        }

        /// <summary>
        /// Uploads the specified file using a request upload token.
        /// </summary>
        /// <param name="fileStream"><see cref="System.IO.FileStream"/> of the file to upload.</param>
        /// <param name="chunkSize">Size (in MB) of the chunks to send.</param>
        /// <param name="progressed">Callback to report upload progress.</param>
        public Task UploadAsync(
            FileStream fileStream,
            double chunkSize = 5D,
            ProgressedDelegate progressed = null)
        {
            var uploadOperation = InnerTusClient.UploadAsync(this.FileUrl, fileStream, chunkSize);

            return PerformUploadAsync(uploadOperation, progressed);
        }

        async Task PerformUploadAsync(TusOperation<List<TusHttpResponse>> uploadOperation, ProgressedDelegate progressed)
        {
            if (progressed != null)
            {
                uploadOperation.Progressed += (transferred, total) =>
                {
                    progressed(transferred, total);
                };
            }

            try
            {
                await uploadOperation;
            }
            catch (AggregateException ex) when (ex.InnerException is TusDotNetClient.TusException tusex)
            {
                var response = TusHelper.ParseResponse(tusex.ResponseContent);

                throw new Exception(response?.Error?.Message ?? tusex.Message, tusex);
            }
        }

        /// <summary>
        /// Uploads the specified file using a request upload token.
        /// </summary>
        /// <param name="fileUrl">Url of the file to upload. Use <seealso cref="RequestUpload"/> to get one.</param>
        /// <param name="requestToken">Request token of the file to upload. Use <seealso cref="RequestUpload"/> to get one.</param>
        /// <param name="fileInfo"><see cref="System.IO.FileInfo"/> of the file to upload.</param>
        /// <param name="chunkSize">Size (in MB) of the chunks to send.</param>
        /// <param name="progressed">Callback to report upload progress.</param>
        public static Task UploadAsync(
            string fileUrl, string requestToken,
            FileInfo fileInfo,
            double chunkSize = 5D,
            ProgressedDelegate progressed = null)
        {
            var builder = new UriBuilder(fileUrl);
            var baseAddress = new Uri(builder.Uri.GetLeftPart(UriPartial.Authority));
            var tusClient = new TusDotNetClient.TusClient();
            var token = new UploadToken { AccessToken = requestToken };
            TusUploaderAsync uploader;

            tusClient.ApplyAuthorization(requestToken);
            uploader = new TusUploaderAsync(baseAddress, tusClient, token, fileUrl);
            return uploader.UploadAsync(fileInfo, chunkSize, progressed);
        }

        /// <summary>
        /// Uploads the specified file using a request upload token.
        /// </summary>
        /// <param name="fileUrl">Url of the file to upload. Use <seealso cref="RequestUpload"/> to get one.</param>
        /// <param name="requestToken">Request token of the file to upload. Use <seealso cref="RequestUpload"/> to get one.</param>
        /// <param name="fileStream"><see cref="System.IO.FileStream"/> of the file to upload.</param>
        /// <param name="chunkSize">Size (in MB) of the chunks to send.</param>
        /// <param name="progressed">Callback to report upload progress.</param>
        public static Task UploadAsync(
            string fileUrl, string requestToken,
            FileStream fileStream,
            double chunkSize = 5D,
            ProgressedDelegate progressed = null)
        {
            var builder = new UriBuilder(fileUrl);
            var baseAddress = new Uri(builder.Uri.GetLeftPart(UriPartial.Authority));
            var tusClient = new TusDotNetClient.TusClient();
            var token = new UploadToken { AccessToken = requestToken };
            TusUploaderAsync uploader;

            tusClient.ApplyAuthorization(requestToken);
            uploader = new TusUploaderAsync(baseAddress, tusClient, token, fileUrl);
            return uploader.UploadAsync(fileStream, chunkSize, progressed);
        }

    }
}
