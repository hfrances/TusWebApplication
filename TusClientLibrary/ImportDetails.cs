using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TusClientLibrary
{
    public sealed class ImportDetails
    {

        public string StoreName { get; internal set; }
        public string BlobId { get; internal set; }
        public string Version { get; internal set; }

        public string FileUrl { get; internal set; }
        public string RelativeUrl { get; internal set; }

    }
}
