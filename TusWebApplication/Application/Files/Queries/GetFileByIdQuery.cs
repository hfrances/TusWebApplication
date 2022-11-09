using MediatR;

namespace TusWebApplication.Application.Files.Queries
{

    public sealed class GetFileByIdQuery : IRequest<Dtos.FileDto>
    {

        public sealed class RequestParameters
        {
            public bool GenerateSas { get; set; }
        }

        public string ContainerName { get; set; }
        public string BlobName { get; set; }
        public RequestParameters Parameters { get; set; }

    }

}
