using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TusClientLibrary
{
    public sealed class FileDetails
    {

        public string StoreName { get; set; }
        public string BlobId { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public string ContentLanguage { get; set; }
        public IDictionary<string, string> Tags { get; set; }
        public IDictionary<string, string> Metadata { get; set; }
        public Uri InnerUrl { get; set; }
        public string Checksum { get; set; }
        public long Length { get; set; }
        public DateTimeOffset CreatedOn { get; set; }

        public string VersionId { get; set; }
        public IEnumerable<FileVersion> Versions { get; set; }

#if NEWTONSOFT
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
#else
        [System.Text.Json.Serialization.JsonConverter(typeof(System.Text.Json.Serialization.JsonStringEnumConverter))]
#endif
        public UploadStatus? Status { get; set; }
        public double? UploadPercentage { get; set; }


    }
}
