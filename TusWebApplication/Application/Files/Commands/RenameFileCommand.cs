using MediatR;

namespace TusWebApplication.Application.Files.Commands
{
    public sealed class RenameFileCommand : IRequest
    {

        public sealed class CommandBody
        {
            string? _blob;

            public string? Blob { set => _blob = value; }
            public string? BlobName { get => _blob; }

        }

        public string StoreName { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public string BlobName { get; set; } = string.Empty;
        public CommandBody? Body { get; set; }

    }
}
