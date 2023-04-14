using MediatR;
using System;

namespace TusWebApplication.Application.Files.Commands
{
    public class GenerateSasTokenFromFileIdCommand : IRequest<string>
    {

        public sealed class RequestBody
        {
            public DateTimeOffset ExpiresOn { get; set; }
        }

        public sealed class RequestParameters
        {
            /// <summary>
            /// Gets or sets the specific version of the blob.
            /// </summary>
            public string? VersionId { get; set; }
        }

        public string StoreName { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public string BlobName { get; set; } = string.Empty;
        public RequestParameters Parameters { get; set; } = new RequestParameters();
        public RequestBody Body { get; set; } = new RequestBody();

    }
}
