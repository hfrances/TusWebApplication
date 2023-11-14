using System;

namespace TusClientLibrary
{
    public sealed class UploadToken
    {

        public string AccessToken { get; set; }
        public DateTimeOffset Expired { get; set; }

    }
}
