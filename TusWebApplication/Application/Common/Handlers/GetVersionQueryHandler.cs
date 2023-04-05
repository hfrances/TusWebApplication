using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using TusWebApplication.Application.Common.Dtos;

namespace TusWebApplication.Application.Common.Queries
{
    sealed class GetVersionQueryHandler : IRequestHandler<GetVersionQuery, VersionDto>
    {

        public async Task<VersionDto> Handle(GetVersionQuery request, CancellationToken cancellationToken)
        {
            return await Task.Run(() =>
            {
                var assembly = Assembly.GetEntryAssembly();

                if (assembly == null)
                {
                    throw new NullReferenceException($"Assembly not found.");
                }
                else
                {
                    var productId = assembly.GetCustomAttribute<GuidAttribute>()?.Value;

                    return new VersionDto
                    {
                        ProductId = productId == null ? (Guid?)null : Guid.Parse(productId),
                        Version = assembly.GetName().Version?.ToString(),
                        OsPlatform = RuntimeInformation.OSDescription,
                        TargetFramework = assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName
                    };
                }
            });
        }
    }
}
