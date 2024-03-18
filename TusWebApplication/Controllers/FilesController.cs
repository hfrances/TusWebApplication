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

    /// <summary>
    /// Provides functions for blob file management.
    /// </summary>
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
        /// <param name="parameters"></param>
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

            if (parameters.Inline)
            {
                // Try visualizate the element in browser. If it is not possible, downloads it.
                var contentDisposition = new System.Net.Mime.ContentDisposition
                {
                    Inline = parameters.Inline,
                    FileName = fileInfo.Name
                };
                Response.Headers.Add("Content-Disposition", contentDisposition.ToString());
                Response.Headers.Add("X-Content-Type-Options", "nosniff");
                return new FileStreamResult(fileInfo.CreateReadStream(), fileInfo.ContentType)
                {
                    EnableRangeProcessing = true
                };
            }
            else
            {
                // Downloads the element directly.
                return File(fileInfo.CreateReadStream(), fileInfo.ContentType, fileInfo.Name);
            }
        }

        /// <summary>
        /// Returns blob details. 
        /// Requires an authentication bearer token created with 'auth' controller.
        /// </summary>
        /// <param name="store">Store name</param>
        /// <param name="container">Container name</param>
        /// <param name="blob">Blob name</param>
        /// <param name="parameters"></param>
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
        /// Requires an authentication bearer token created with 'auth' controller.
        /// </summary>
        /// <param name="store">Store name</param>
        /// <param name="container">Container name</param>
        /// <param name="blob">Blob name</param>
        /// <param name="parameters"></param>
        /// <param name="body"></param>
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
        /// Requires an authentication bearer token created with 'auth' controller.
        /// </summary>
        /// <param name="store">Store name</param>
        /// <param name="container">Container name</param>
        /// <param name="blob">Blob name</param>
        /// <param name="body"></param>
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
        /// Requires an authentication bearer token created with 'auth' controller.
        /// </summary>
        /// <param name="store">Store name</param>
        /// <param name="container">Container name</param>
        /// <param name="body"></param>
        [HttpPost("{store}/{container}/request-upload"), Authorize, IpSafeFilter]
        public Task<RequestUploadDto> RequestUpload(string store, string container, [FromBody] RequestUploadCommand.CommandBody body)
            => Send(new RequestUploadCommand
            {
                StoreName = store,
                Container = container,
                Body = body
            });

        /// <summary>
        /// Takes a file from other blob storage and imports it in the specific container.
        /// Requires an authentication bearer token created with 'auth' controller.
        /// </summary>
        /// <param name="store">Store name</param>
        /// <param name="container">Container name</param>
        /// <param name="body"></param>
        [HttpPost("{store}/{container}/import"), Authorize, IpSafeFilter]
        public Task<ImportDto> GenerateSas(string store, string container, [FromBody] ImportFileCommand.CommandBody body)
            => Send(new ImportFileCommand
            {
                StoreName = store,
                ContainerName = container,
                Body = body
            });

        /// <summary>
        /// Deletes the specific blob.
        /// Requires an authentication bearer token created with 'auth' controller.
        /// </summary>
        /// <param name="store">Store name</param>
        /// <param name="container">Container name</param>
        /// <param name="blob">Blob name</param>
        /// <param name="parameters"></param>
        [HttpDelete("{store}/{container}/{blob}"), Authorize, IpSafeFilter]
        public Task GenerateSas(string store, string container, string blob, [FromQuery] DeleteFileCommand.RequestParameters parameters)
            => Send(new DeleteFileCommand
            {
                StoreName = store,
                ContainerName = container,
                BlobName = blob,
                Parameters = parameters
            });

    }

}
