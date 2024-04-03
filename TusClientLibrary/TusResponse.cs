using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TusClientLibrary
{
    sealed class TusResponse
    {

        public sealed class TusError
        {

            public string Message { get; set; }
            public TusError InnerError { get; set; }

        }

        public TusError Error { get; set; }

    }
}
