using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace TusWebApplication.Controllers
{

    [Route("api/files")]
    [ApiController, AllowAnonymous]
    public sealed class FilesController : Base.ApiControllerBase
    {

        /// <summary>
        /// Returns assembly the assembly version.
        /// </summary>
        [HttpGet("{fileId}")]
        public async Task GetFilebyIdAsync(string fileId)
        {
            await Task.Run(() => System.Diagnostics.Debug.WriteLine($"GetFile: {fileId}"));
        }

    }

}
