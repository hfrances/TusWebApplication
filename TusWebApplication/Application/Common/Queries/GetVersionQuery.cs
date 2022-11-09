using MediatR;
using TusWebApplication.Application.Common.Dtos;

namespace TusWebApplication.Application.Common.Queries
{
    sealed class GetVersionQuery : IRequest<VersionDto>
    {
    }
}
