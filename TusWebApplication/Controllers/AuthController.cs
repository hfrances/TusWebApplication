using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using qckdev.AspNetCore.Mvc.Filters.IpSafe;
using System.Threading.Tasks;
using TusWebApplication.Application.Auth.Commands;
using TusWebApplication.Application.Auth.Dtos;
using TusWebApplication.Application.Auth.Queries;

namespace TusWebApplication.Controllers
{

    [Route("api/auth")]
    [ApiController, Authorize, IpSafeFilter]
    public class AuthController : Base.ApiControllerBase
    {

        [HttpPost, AllowAnonymous]
        public Task<TokenDto> Login([FromBody] LoginCommand request)
            => Send(request);

        [HttpGet]
        public Task<LoginDto> Login()
            => Send(new GetUserNameQuery());

    }
}
