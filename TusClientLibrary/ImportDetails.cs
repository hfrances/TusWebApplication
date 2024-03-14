using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TusClientLibrary
{
    public sealed class ImportDetails
    {

        public string StoreName { get; set; }
        public string BlobId { get; set; }
        public string Version { get; set; }

        public string FileUrl { get; set; }
        public string RelativeUrl => new Uri(FileUrl).AbsolutePath;

    }
}
