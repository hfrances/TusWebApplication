using MediatR;
using System;
using System.Collections.Generic;
using TusWebApplication.Application.Files.Dtos;

namespace TusWebApplication.Application.Files.Commands
{
    public class GenerateSasTokenFromArrayCommand : IRequest<IEnumerable<TokenSasDto>>
    {

        public sealed class BlobInfo
        {
            public string BlobName { get; set; } = string.Empty;
            /// <summary>
            /// Gets or sets the specific version of the blob.
            /// </summary>
            public string? VersionId { get; set; }
        }

        public sealed class RequestBody
        {
            public DateTimeOffset ExpiresOn { get; set; }
            public IEnumerable<BlobInfo> Blobs { get; set; } = Array.Empty<BlobInfo>();
        }

        public string StoreName { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public RequestBody Body { get; set; } = new RequestBody();

    }
}
