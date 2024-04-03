using System;

namespace TusClientLibrary
{
    sealed class Token
    {

        public string AccessToken { get; set; }
        public DateTimeOffset Expired { get; set; }

    }
}
