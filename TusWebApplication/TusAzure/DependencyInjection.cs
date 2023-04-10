using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using tusdotnet;
using tusdotnet.Models;

namespace TusWebApplication.TusAzure
{
    static class DependencyInjection
    {

        public static IServiceCollection AddTusAzure(this IServiceCollection services)
        {
            
            services.AddSingleton(factory =>
            {
                var tusAzureStores = new TusAzureStoreDictionary();
                var settings = factory.GetService<IOptions<AzureBlobProvider.AzureStorageCredentialsSettings>>()?.Value;

                if (settings != null)
                {
                    foreach (var pair in settings)
                    {
                        tusAzureStores.Add(pair.Key, new TusAzureStoreQueued(
                            pair.Value?.AccountName ?? string.Empty,
                            pair.Value?.AccountKey ?? string.Empty,
                            pair.Value?.DefaultContainer ?? string.Empty
                        ));
                    }
                }
                return tusAzureStores;
            });
            return services;
        }

        public static IApplicationBuilder UseTusAzure(this IApplicationBuilder app, string basePath)
        {
            var tusAzureStores = app.ApplicationServices.GetService<TusAzureStoreDictionary>();

            if (tusAzureStores != null)
            {
                foreach (var (name, store) in tusAzureStores)
                {
                    app.UseTus(httpContext => new DefaultTusConfiguration
                    {
                        Store = store,
                        UrlPath = CombineUrl(basePath, $"/api/files/{name}"),
                        MetadataParsingStrategy = MetadataParsingStrategy.AllowEmptyValues,
                        UsePipelinesIfAvailable = true,
                    });
                }
            }
            return app;
        }

        private static string CombineUrl(string basePath, string urlPath)
        {
            string rdo;

            if (string.IsNullOrEmpty(basePath))
            {
                rdo = urlPath;
            }
            else
            {
                int startIndex = 0;

                if (basePath.EndsWith("/") && urlPath.StartsWith("/"))
                {
                    startIndex = 1;
                }
                rdo = $"{basePath}{urlPath[startIndex..]}";
            }
            return rdo;
        }

    }
}
