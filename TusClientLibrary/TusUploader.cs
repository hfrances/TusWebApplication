using TusDotNetClientSync = qckdev.Storage.TusDotNetClientSync;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static TusClientLibrary.TusClient;

namespace TusClientLibrary
{
    public sealed class TusUploader
    {

        TusDotNetClientSync.TusClient InnerTusClient { get; }

        public Uri BaseAddress { get; }
        public UploadToken UploadToken { get; }
        public string StoreName => UploadToken.StoreName;
        public string BlobId => UploadToken.BlobId;
        public string FileUrl { get; }
        public string RelativeUrl => BaseAddress.MakeRelativeUri(new Uri(FileUrl)).ToString();

        internal TusUploader(Uri baseAddress, TusDotNetClientSync.TusClient tusClient, UploadToken uploadToken, string fileUrl)
        {
            this.InnerTusClient = tusClient;
            this.BaseAddress = baseAddress;
            this.UploadToken = uploadToken;
            this.FileUrl = fileUrl;
        }

        /// <summary>
        /// Uploads the specified file using a request upload token.
        /// </summary>
        /// <param name="fileInfo"><see cref="System.IO.FileInfo"/> of the file to upload.</param>
        /// <param name="chunkSize">Size (in MB) of the chunks to send.</param>
        /// <param name="progressed">Callback to report upload progress.</param>
        public void Upload(
            FileInfo fileInfo,
            double chunkSize = 5D,
            ProgressedDelegate progressed = null)
        {
            var uploadOperation = InnerTusClient.UploadAsync(this.FileUrl, fileInfo, chunkSize);

            PerformUpload(uploadOperation, progressed);
        }

        /// <summary>
        /// Uploads the specified file using a request upload token.
        /// </summary>
        /// <param name="fileStream"><see cref="System.IO.Stream"/> of the file to upload.</param>
        /// <param name="chunkSize">Size (in MB) of the chunks to send.</param>
        /// <param name="progressed">Callback to report upload progress.</param>
        public void Upload(
            Stream fileStream,
            double chunkSize = 5D,
            ProgressedDelegate progressed = null)
        {
            var uploadOperation = InnerTusClient.Upload(this.FileUrl, fileStream, chunkSize);

            PerformUpload(uploadOperation, progressed);
        }

        void PerformUpload(TusDotNetClientSync.TusOperation<List<TusDotNetClientSync.TusHttpResponse>> uploadOperation, ProgressedDelegate progressed)
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
                uploadOperation.Get();
            }
            catch (Exception ex) when (ex.InnerException is TusDotNetClientSync.TusException tusex)
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
        public static void Upload(
            string fileUrl, string requestToken,
            FileInfo fileInfo,
            double chunkSize = 5D,
            ProgressedDelegate progressed = null)
        {
            var builder = new UriBuilder(fileUrl);
            var baseAddress = new Uri(builder.Uri.GetLeftPart(UriPartial.Authority));
            var tusClient = new TusDotNetClientSync.TusClient();
            var token = new UploadToken { AccessToken = requestToken };
            TusUploader uploader;

            tusClient.ApplyAuthorization(requestToken);
            uploader = new TusUploader(baseAddress, tusClient, token, fileUrl);
            uploader.Upload(fileInfo, chunkSize, progressed);
        }

        /// <summary>
        /// Uploads the specified file using a request upload token.
        /// </summary>
        /// <param name="fileUrl">Url of the file to upload. Use <seealso cref="RequestUpload"/> to get one.</param>
        /// <param name="requestToken">Request token of the file to upload. Use <seealso cref="RequestUpload"/> to get one.</param>
        /// <param name="fileStream"><see cref="System.IO.Stream"/> of the file to upload.</param>
        /// <param name="chunkSize">Size (in MB) of the chunks to send.</param>
        /// <param name="progressed">Callback to report upload progress.</param>
        public static void Upload(
            string fileUrl, string requestToken,
            Stream fileStream,
            double chunkSize = 5D,
            ProgressedDelegate progressed = null)
        {
            var builder = new UriBuilder(fileUrl);
            var baseAddress = new Uri(builder.Uri.GetLeftPart(UriPartial.Authority));
            var tusClient = new TusDotNetClientSync.TusClient();
            var token = new UploadToken { AccessToken = requestToken };
            TusUploader uploader;

            tusClient.ApplyAuthorization(requestToken);
            uploader = new TusUploader(baseAddress, tusClient, token, fileUrl);
            uploader.Upload(fileStream, chunkSize, progressed);
        }

    }
}
