using MediatR;

namespace TusWebApplication.Application.Files.Queries
{

    public sealed class GetFileByIdQuery : IRequest<Dtos.FileDto>
    {

        public sealed class RequestParameters
        {
            /// <summary>
            /// Gets or sets the specific version of the blob.
            /// </summary>
            public string? VersionId { get; set; }
            /// <summary>
            /// Gets or sets if it must load version list.
            /// </summary>
            public bool LoadVersions { get; set; }
        }

        public string StoreName { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public string BlobName { get; set; } = string.Empty;
        public RequestParameters Parameters { get; set; } = new RequestParameters();

    }

}
