using MediatR;
using System;

namespace TusWebApplication.Application.Files.Queries
{
    public sealed class DownloadFileQuery : IRequest<TusWebApplication.AzureBlobProvider.IDownloadableFileInfo>
    {

        public sealed class RequestParameters
        {
            /// <summary>
            /// Sends Content-Disposition "inline" when true, or "attachment" otherwise (default true).
            /// </summary>
            public bool Inline { get; set; } = true;
            /// <summary>
            /// Gets or sets the specific version of the blob.
            /// </summary>
            public string? VersionId { get; set; }
            /// <summary>
            /// Sas token version.
            /// </summary>
            public string? Sv { get; set; }
            /// <summary>
            /// Expires on in UTC format.
            /// </summary>
            public DateTimeOffset? Se { get; set; }
            /// <summary>
            /// Signature for validating Sas token.
            /// </summary>
            public string? Sig { get; set; }
        }

        public string StoreName { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public string BlobName { get; set; } = string.Empty;
        public RequestParameters Parameters { get; set; } = new RequestParameters();


    }
}
