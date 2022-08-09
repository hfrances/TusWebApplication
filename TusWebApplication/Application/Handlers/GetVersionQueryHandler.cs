using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TusWebApplication.Application.Dtos;

namespace TusWebApplication.Application.Queries
{
    sealed class GetVersionQueryHandler : IRequestHandler<GetVersionQuery, VersionDto>
    {

        public async Task<VersionDto> Handle(GetVersionQuery request, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                Version? version;
                var assembly = System.Reflection.Assembly.GetEntryAssembly();

                if (assembly == null)
                {
                    throw new NullReferenceException($"Assembly not found.");
                }
                else
                {
                    version = assembly.GetName().Version;
                }

                return new VersionDto
                {
                    Version = version?.ToString()
                };
            });
        }
    }
}
