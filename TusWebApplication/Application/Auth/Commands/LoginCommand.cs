using MediatR;
using TusWebApplication.Application.Auth.Dtos;

namespace TusWebApplication.Application.Auth.Commands
{

    public sealed class LoginCommand : IRequest<TokenDto>
    {

        public string UserName { get; set; } = string.Empty;
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

    }

}
