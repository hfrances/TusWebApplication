using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TusWebApplication.Application.Files.Dtos
{
    public sealed class FileDto
    {

        public enum UploadStatus
        {
            Unknown,
            Uploading,
            Done,
            Error
        }

        public string StoreName { get; set; } = string.Empty;

        public string BlobId { get; set; } = string.Empty;

        public string? Name { get; set; }

        public string ContentType { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ContentLanguage { get; set; }

        public IDictionary<string, string>? Tags { get; set; }
        public IDictionary<string, string>? Metadata { get; set; }

        /// <summary>
        /// Gets the blob's primary <see cref="Uri"/> endpoint.
        /// </summary>
        public Uri InnerUrl { get; set; } = null!;

        public string? Checksum { get; set; }

        public long Length { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DateTimeOffset CreatedOn { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? VersionId { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<FileVersionDto>? Versions { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public UploadStatus? Status { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public double? UploadPercentage { get; set; }

    }
}
