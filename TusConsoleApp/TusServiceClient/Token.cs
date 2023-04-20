using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TusConsoleApp.TusServiceClient
{
    sealed class Token
    {

        public string AccessToken { get; set; }
        public DateTimeOffset Expired { get; set; }

    }
}
