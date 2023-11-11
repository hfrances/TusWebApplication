using System;

namespace TusClientLibrary
{
    sealed class UploadToken
    {

        public string AccessToken { get; set; }
        public DateTimeOffset Expired { get; set; }

    }
}
