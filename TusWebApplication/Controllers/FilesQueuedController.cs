using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TusWebApplication.Application.Files.Commands;
using TusWebApplication.Application.Files.Dtos;
using TusWebApplication.Application.Files.Queries;

namespace TusWebApplication.Controllers
{

    [Route("api/filesQueued")]
    [ApiController, AllowAnonymous]
    public sealed class FilesQueuedController : Base.ApiControllerBase
    {

        /// <summary>
        /// Returns assembly the assembly version.
        /// </summary>
        [HttpGet("{container}/{blob}")]
        public Task<FileDto> GetFilebyIdAsync(string container, string blob, [FromQuery] GetFileByIdQuery.RequestParameters parameters)
            => Send(new GetFileByIdQuery
            {
                ContainerName = container,
                BlobName = blob,
                Parameters = parameters
            });

        /// <summary>
        /// Renames a file.
        /// </summary>
        [HttpPut("{container}/{blob}/rename")]
        public Task RenameFileAsync(string container, string blob, [FromBody] RenameFileCommand.CommandBody body)
            => Send(new RenameFileCommand
            {
                ContainerName = container,
                BlobName = blob,
                Body = body
            });

    }

}
