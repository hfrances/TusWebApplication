using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using qckdev.AspNetCore.Mvc.Filters.IpSafe;
using System.Threading.Tasks;
using TusWebApplication.Application.Auth.Commands;
using TusWebApplication.Application.Auth.Dtos;
using TusWebApplication.Application.Auth.Queries;

namespace TusWebApplication.Controllers
{

    [Route("auth")]
    [ApiController, Authorize, IpSafeFilter]
    public class AuthController : Base.ApiControllerBase
    {

        /// <summary>
        /// Generates an authentication token bearer that is necessary for some protected operations.
        /// </summary>
        /// <param name="request"></param>
        [HttpPost, AllowAnonymous]
        public Task<TokenDto> Login([FromBody] LoginCommand request)
            => Send(request);

        /// <summary>
        /// Retrieves the information about an authorized token bearer.
        /// </summary>
        [HttpGet]
        public Task<LoginDto> Login()
            => Send(new GetUserNameQuery());

    }
}
