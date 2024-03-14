using System;

namespace TusWebApplication.Application.Files.Dtos
{
    public sealed class RequestUploadDto
    {

        public string StoreName { get; set; } = string.Empty;
        public string BlobId { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public DateTimeOffset Expired { get; set; }

    }
}
