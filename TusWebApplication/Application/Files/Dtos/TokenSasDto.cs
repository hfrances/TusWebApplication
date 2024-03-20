using System.Text.Json.Serialization;

namespace TusWebApplication.Application.Files.Dtos
{
    public class TokenSasDto
    {

        public string StoreName { get; internal set; } = string.Empty;

        public string ContainerName { get; internal set; } = string.Empty;

        public string BlobName { get; internal set; } = string.Empty;

        public string? Version { get; internal set; } = string.Empty;

        /// <summary>
        /// Gets the token SAS for the blob file or null if the blob does not exist.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.Never)]
        public string? TokenSas { get; internal set; } = string.Empty;

    }
}
