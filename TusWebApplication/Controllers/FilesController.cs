using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using qckdev.AspNetCore.Mvc.Filters.IpSafe;
using System;
using System.Threading.Tasks;
using TusWebApplication.Application.Files.Commands;
using TusWebApplication.Application.Files.Dtos;
using TusWebApplication.Application.Files.Queries;

namespace TusWebApplication.Controllers
{

    [Route("files")]
    [ApiController]
    public class FilesController : Base.ApiControllerBase
    {

        /// <summary>
        /// Downloads a specific blob.
        /// </summary>
        /// <param name="store">Store name</param>
        /// <param name="container">Container name</param>
        /// <param name="blob">Blob name</param>
        [HttpGet("{store}/{container}/{blob}"), AllowAnonymous]
        public async Task<IActionResult> Download(string store, string container, string blob, [FromQuery] DownloadFileQuery.RequestParameters parameters)
        {
            var fileInfo = await Send(new DownloadFileQuery
            {
                StoreName = store,
                ContainerName = container,
                BlobName = blob,
                Parameters = parameters
            });

            return File(fileInfo.CreateReadStream(), fileInfo.ContentType, fileInfo.Name);
        }

        /// <summary>
        /// Returns blob details.
        /// </summary>
        /// <param name="store">Store name</param>
        /// <param name="container">Container name</param>
        /// <param name="blob">Blob name</param>
        [HttpGet("{store}/{container}/{blob}/details"), Authorize, IpSafeFilter]
        public Task<FileDto> GetFilebyIdAsync(string store, string container, string blob, [FromQuery] GetFileByIdQuery.RequestParameters parameters)
            => Send(new GetFileByIdQuery
            {
                StoreName = store,
                ContainerName = container,
                BlobName = blob,
                Parameters = parameters
            });

        /// <summary>
        /// Returns an url that includes a temporal shared access signature.
        /// </summary>
        /// <param name="store">Store name</param>
        /// <param name="container">Container name</param>
        /// <param name="blob">Blob name</param>
        [HttpPost("{store}/{container}/{blob}/generateSas"), Authorize, IpSafeFilter]
        public Task<string> GenerateSas(string store, string container, string blob, [FromQuery] GenerateSasTokenFromFileIdCommand.RequestParameters parameters, [FromBody] GenerateSasTokenFromFileIdCommand.RequestBody body)
            => Send(new GenerateSasTokenFromFileIdCommand
            {
                StoreName = store,
                ContainerName = container,
                BlobName = blob,
                Parameters = parameters,
                Body = body
            });

        /// <summary>
        /// Renames a blob.
        /// </summary>
        /// <param name="store">Store name</param>
        /// <param name="container">Container name</param>
        /// <param name="blob">Blob name</param>
        [HttpPut("{store}/{container}/{blob}/rename"), Authorize, IpSafeFilter]
        public Task RenameFileAsync(string store, string container, string blob, [FromBody] RenameFileCommand.CommandBody body)
            => Send(new RenameFileCommand
            {
                StoreName = store,
                ContainerName = container,
                BlobName = blob,
                Body = body
            });

        /// <summary>
        /// Creates a temporal token for uploading an specific file.
        /// </summary>
        /// <param name="store">Store name</param>
        /// <param name="container">Container name</param>
        [HttpPost("{store}/{container}/request-upload"), Authorize, IpSafeFilter]
        public Task<RequestUploadDto> RequestUpload(string store, string container, [FromBody] RequestUploadCommand.CommandBody body)
            => Send(new RequestUploadCommand
            {
                StoreName = store,
                Container = container,
                Body = body
            });

    }

}
