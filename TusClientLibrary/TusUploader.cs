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
        public FileInfo FileInfo { get; }
        public string FileUrl { get; }

        internal TusUploader(Uri baseAddress, TusDotNetClient.TusClient tusClient, UploadToken uploadToken, FileInfo fileInfo, string fileUrl)
        {
            this.InnerTusClient = tusClient;
            this.BaseAddress = baseAddress;
            this.UploadToken = uploadToken;
            this.FileInfo = fileInfo;
            this.FileUrl = fileUrl;

            this.InnerHttpClient = new HttpClient() { BaseAddress = baseAddress };
        }

        public void Upload(
            double chunkSize = 5D,
            ProgressedDelegate progressed = null)
        {
            var uploadOperation = InnerTusClient.UploadAsync(this.FileUrl, this.FileInfo, chunkSize);

            if (progressed != null)
            {
                uploadOperation.Progressed += (transferred, total) =>
                {
                    progressed(transferred, total);
                };
            }
            uploadOperation.Operation.Wait();
        }


    }
}
