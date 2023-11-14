using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static TusClientLibrary.TusClient;

namespace TusClientLibrary
{
    public sealed class TusUploader
    {

        UploadToken UploadToken { get; }
        TusDotNetClient.TusClient InnerTusClient { get; }
        HttpClient InnerHttpClient { get; }
        
        public Uri BaseAddress { get; }
        public string FileUrl { get; }
        public string RelativeUrl => new Uri(FileUrl).AbsolutePath;

        internal TusUploader(Uri baseAddress, TusDotNetClient.TusClient tusClient, UploadToken uploadToken, string fileUrl)
        {
            this.InnerTusClient = tusClient;
            this.BaseAddress = baseAddress;
            this.UploadToken = uploadToken;
            this.FileUrl = fileUrl;

            this.InnerHttpClient = new HttpClient() { BaseAddress = baseAddress };
        }

        public void Upload(
            FileInfo fileInfo,
            double chunkSize = 5D,
            ProgressedDelegate progressed = null)
        {
            var uploadOperation = InnerTusClient.UploadAsync(this.FileUrl, fileInfo, chunkSize);

            if (progressed != null)
            {
                uploadOperation.Progressed += (transferred, total) =>
                {
                    progressed(transferred, total);
                };
            }

            try
            {
                uploadOperation.Operation.Wait();
            }
            catch (AggregateException ex) when (ex.InnerException is TusDotNetClient.TusException tusex)
            {
                var response = qckdev.Text.Json.JsonConvert.DeserializeObject<TusResponse>(tusex.ResponseContent);

                throw new Exception(response.Error?.Message ?? tusex.Message, tusex);
            }
        }

    }
}
