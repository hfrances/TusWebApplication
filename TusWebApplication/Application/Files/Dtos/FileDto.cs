using System;
using System.Collections.Generic;

namespace TusWebApplication.Application.Files.Dtos
{
    public sealed class FileDto
    {

        public string BlobId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public IDictionary<string, string>? Tags { get; set; }
        public IDictionary<string, string>? Metadata { get; set; }
        public Uri? Url { get; set; }
        public string? Checksum { get; set; }
        public long Length { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
    }
}
