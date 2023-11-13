using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TusClientLibrary
{
    public sealed class FileDetails
    {

        public string BlobId { get; set; }
        public string Name { get; set; }
        public IDictionary<string, string> Tags { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public Uri Url { get; set; }
        public string Checksum { get; set; }
        public long Length { get; set; }
        public DateTimeOffset CreatedOn { get; set; }

        public string VersionId { get; set; }
        public IEnumerable<FileVersion> Versions { get; set; }

        public UploadStatus? Status { get; set; }
        public double? UploadPercentage { get; set; }


    }
}
