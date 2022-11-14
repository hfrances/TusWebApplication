using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using qckdev.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using tusdotnet;
using tusdotnet.Models;
using TusWebApplication.Application;
using TusWebApplication.Swagger;

namespace TusWebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Configuration.ApplyEnvironmentVariables();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var azureStorageCredentialSettings = this.Configuration.GetSection("AzureStorageCredential").Get<TusAzure.AzureStorageCredentialSettings>();
            var azureStorageCredentialSettings2 = this.Configuration.GetSection("AzureStorageCredential").Get<AzureBlobProvider.AzureStorageCredentialSettings>();


            services.AddCors(opts => opts.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            services.AddApplication();
            services.AddScoped<AzureBlobProvider.AzureBlobFileProvider>();
            services.AddSingleton(x => new TusAzure.TusAzureStoreQueued(
                azureStorageCredentialSettings.AccountName ?? string.Empty,
                azureStorageCredentialSettings.AccountKey ?? string.Empty,
                azureStorageCredentialSettings.DefaultContainer ?? string.Empty
            ));
            services.AddSingleton(x => new TusAzure.TusAzureStore(
                azureStorageCredentialSettings.AccountName ?? string.Empty,
                azureStorageCredentialSettings.AccountKey ?? string.Empty,
                azureStorageCredentialSettings.DefaultContainer ?? string.Empty
            ));
            services.Configure(azureStorageCredentialSettings);
            services.Configure(azureStorageCredentialSettings2);

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            services.AddSwagger();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var basePath = this.Configuration.GetSection("BasePath")?.Value ?? "/";
            var combineUrl = new Func<string, string, string>((string basePath, string urlPath) =>
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
            });

            app.UseCors();

            app.UsePathBase(basePath);
            if (env.IsDevelopment() || env.IsStaging())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
            }

            app.UseRouting();
            app.UseAuthorization();

            app.UseSerializedExceptionHandler();

            app.UseTus(httpContext => new DefaultTusConfiguration
            {
                Store = app.ApplicationServices.GetService<TusAzure.TusAzureStoreQueued>(),
                UrlPath = combineUrl(basePath, "/api/filesQueued"),
                MetadataParsingStrategy = MetadataParsingStrategy.AllowEmptyValues,
                UsePipelinesIfAvailable = true,
            });

            app.UseTus(httpContext => new DefaultTusConfiguration
            {
                Store = app.ApplicationServices.GetService<TusAzure.TusAzureStore>(),
                UrlPath = combineUrl(basePath, "/api/files"),
                MetadataParsingStrategy = MetadataParsingStrategy.AllowEmptyValues,
                UsePipelinesIfAvailable = true,
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
