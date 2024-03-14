using MediatR;
using System.Collections;
using System.Collections.Generic;

namespace TusWebApplication.Application.Files.Commands
{
    public sealed class ImportFileCommand : IRequest
    {

        public sealed class CommandBody
        {
            public string? TargetBlobName { get; set; }

            public string SourceUrl { get; set; } = string.Empty;

            public string FileName { get; set; } = string.Empty;

            /// <summary>
            /// Optional. Gets or sets the blob content type to replace when it is copying. Null for leave the original one.
            /// </summary>
            public string? ContentType { get; set; }

            public IDictionary<string, string>? Tags { get; set; }

            public IDictionary<string, string>? Metadata { get; set; }

            public bool WaitForComplete { get; set; }

        }

        public string StoreName { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public CommandBody? Body { get; set; }
    }
}
