using MediatR;
using TusWebApplication.Application.Dtos;

namespace TusWebApplication.Application.Queries
{
    sealed class GetVersionQuery : IRequest<VersionDto>
    {
    }
}
