using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TusWebApplication.Application.Dtos;
using TusWebApplication.Application.Queries;

namespace TusWebApplication.Controllers
{

    [Route("api/common")]
    [ApiController, AllowAnonymous]
    public sealed class CommonController : Base.ApiControllerBase
    {

        /// <summary>
        /// Returns assembly the assembly version.
        /// </summary>
        [HttpGet("version")]
        public async Task<VersionDto> GetVersionAsync()
        {
            return await Send(new GetVersionQuery());
        }

    }
}
