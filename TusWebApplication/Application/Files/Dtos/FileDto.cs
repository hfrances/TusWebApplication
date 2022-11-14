using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TusWebApplication.Application.Files.Dtos
{
    public sealed class FileDto
    {

        public string BlobId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public IDictionary<string, string>? Tags { get; set; }
        public IDictionary<string, string>? Metadata { get; set; }
        public Uri? Url { get; set; }
        public string? Checksum { get; set; }
        public long Length { get; set; }
        public DateTimeOffset CreatedOn { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? VersionId { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<FileVersionDto>? Versions { get; set; }
        
    }
}
