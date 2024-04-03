using System.Text.Json.Serialization;

namespace TusWebApplication.Application.Files.Dtos
{
    public sealed class ImportDto
    {

        public string StoreName { get; set; } = string.Empty;

        public string BlobId { get; set; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? VersionId { get; set; }

    }
}
