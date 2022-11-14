using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TusWebApplication.Controllers
{

    [Route("api/filesQueued")]
    [ApiController, AllowAnonymous]
    public sealed class FilesQueuedController : FilesController
    {

        public FilesQueuedController(AzureBlobProvider.AzureBlobFileProvider azureBlobFileProvider)
            : base(azureBlobFileProvider)
        { }

    }

}
