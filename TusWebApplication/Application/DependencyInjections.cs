using Microsoft.Extensions.DependencyInjection;
using MediatR;
using System.Collections;

namespace TusWebApplication.Application
{
    static class DependencyInjections
    {

        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services
                .AddScoped<AzureBlobProvider.AzureBlobFileProvider>()
                .AddMediatR(System.Reflection.Assembly.GetExecutingAssembly())
                .AddHttpContextAccessor()
            ;
            return services;
        }

    }
}
