using System;

namespace TusClientLibrary
{
    public class UploadToken
    {

        public string StoreName { get; set; }
        public string BlobId { get; set; }
        public string AccessToken { get; set; }
        public DateTimeOffset Expired { get; set; }

    }
}
