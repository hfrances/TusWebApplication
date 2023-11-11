using System;

namespace TusWebApplication.Application.Files.Dtos
{
    public sealed class RequestUploadDto
    {

        public string AccessToken { get; set; } = string.Empty;
        public DateTimeOffset Expired { get; set; }

    }
}
