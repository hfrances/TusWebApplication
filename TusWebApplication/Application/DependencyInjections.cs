using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace TusWebApplication.Application
{
    static class DependencyInjections
    {

        public static IServiceCollection AddApplication(this IServiceCollection service)
        {
            service
                .AddMediatR(System.Reflection.Assembly.GetExecutingAssembly())
                .AddHttpContextAccessor()
            ;
            return service;
        }

        public static IServiceCollection Configure<TConfiguration>(this IServiceCollection services, TConfiguration configuration) where TConfiguration : class
        {
            OptionsServiceCollectionExtensions.Configure<TConfiguration>(services, config =>
            {
                new AutoMapper.MapperConfiguration(config => config.CreateMap<TConfiguration, TConfiguration>())
                    .CreateMapper()
                    .Map(configuration, config);
            });
            return services;
        }

    }
}
