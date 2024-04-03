using MediatR;
using TusWebApplication.Application.Auth.Dtos;

namespace TusWebApplication.Application.Auth.Queries
{

    public sealed class GetUserNameQuery : IRequest<LoginDto>
    {

    }

}
