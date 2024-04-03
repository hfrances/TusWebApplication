using MediatR;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using TusWebApplication.Application.Auth.Dtos;
using TusWebApplication.Application.Auth.Queries;

namespace TusWebApplication.Application.Auth.Handlers
{

    sealed class GetUserNameQueryHandler : IRequestHandler<GetUserNameQuery, LoginDto>
    {

        IHttpContextAccessor HttpContextAccessor { get; }

        public GetUserNameQueryHandler(IHttpContextAccessor httpContextAccessor)
        {
            this.HttpContextAccessor = httpContextAccessor;
        }

        public Task<LoginDto> Handle(GetUserNameQuery request, CancellationToken cancellationToken)
        {
            var httpContext = HttpContextAccessor.HttpContext;
            var currentUser = httpContext.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(currentUser))
            {
                throw new Exceptions.LoginException();
            }
            else
            {
                return Task.FromResult(new LoginDto
                {
                    UserName = currentUser
                });
            }
        }
    }

}
