using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Threading.Tasks;
using TusWebApplication.Application.Files.Commands;
using TusWebApplication.Application.Files.Dtos;
using TusWebApplication.Application.Files.Queries;

namespace TusWebApplication.Controllers
{

    [Route("api/files")]
    [ApiController, AllowAnonymous]
    public class FilesController : Base.ApiControllerBase
    {

        AzureBlobProvider.AzureBlobFileProvider AzureBlobFileProvider { get; }

        public FilesController(AzureBlobProvider.AzureBlobFileProvider azureBlobFileProvider)
        {
            this.AzureBlobFileProvider = azureBlobFileProvider;
        }

        /// <summary>
        /// Returns assembly the assembly version.
        /// </summary>
        [HttpGet("{container}/{blob}/details")]
        public Task<FileDto> GetFilebyIdAsync(string container, string blob, [FromQuery] GetFileByIdQuery.RequestParameters parameters)
            => Send(new GetFileByIdQuery
            {
                ContainerName = container,
                BlobName = blob,
                Parameters = parameters
            });

        [HttpGet("{container}/{blob}")]
        public async Task<IActionResult> Download(string container, string blob)
        {
            var rdo = await GetFilebyIdAsync(container, blob, new GetFileByIdQuery.RequestParameters
            {
                GenerateSas = true
            });

            if (rdo.Url != null)
            {
                var fileInfo = AzureBlobFileProvider.GetFileInfo($"{container}/{blob}");

                return File(fileInfo.CreateReadStream(), "application/octet-stream", fileInfo.Name);
            }
            else
            {
                return StatusCode(500);
            }
        }


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
