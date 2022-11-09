using MediatR;

namespace TusWebApplication.Application.Files.Queries
{

    public sealed class GetFileByIdQuery : IRequest<Dtos.FileDto>
    {

        public sealed class RequestParameters
        {
            public string? VersionId { get; set; }
            public bool GenerateSas { get; set; }
            public bool LoadVersions { get; set; }
        }

        public string ContainerName { get; set; } = string.Empty;
        public string BlobName { get; set; } = string.Empty;
        public RequestParameters Parameters { get; set; } = new RequestParameters();

    }

}
