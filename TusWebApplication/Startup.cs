﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.IO;
using tusdotnet;
using tusdotnet.Interfaces;
using tusdotnet.Models;
using tusdotnet.Stores;
using TusWebApplication.Application;
using TusWebApplication.Swagger;

namespace TusWebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddCors(opts => opts.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            services.AddApplication();
            services.AddControllers();
            services.AddSwagger();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors();

            if (env.IsDevelopment() || env.IsStaging())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
            }

            app.UseRouting();
            app.UseAuthorization();

            var azureStorageCredentialSettings = this.Configuration.GetSection("AzureStorageCredential").Get<AzureStorageCredentialSettings>();
            var store = new TusAzureStoreQueued(
                azureStorageCredentialSettings.AccountName ?? string.Empty, 
                azureStorageCredentialSettings.AccountKey ?? string.Empty, 
                azureStorageCredentialSettings.DefaultContainer ?? string.Empty
            );

            app.UseTus(httpContext => new DefaultTusConfiguration
            {
                // This method is called on each request so different configurations can be returned per user, domain, path etc.
                // Return null to disable tusdotnet for the current request.

                // c:\tusfiles is where to store files
                Store = store,
                // On what url should we listen for uploads?
                UrlPath = "/api/files",
                MetadataParsingStrategy = MetadataParsingStrategy.AllowEmptyValues,
                UsePipelinesIfAvailable = true,
                Events = new tusdotnet.Models.Configuration.Events
                {
                    //OnFileCompleteAsync = async eventContext =>
                    //{
                    //    ITusFile file = await eventContext.GetFileAsync();
                    //    Dictionary<string, Metadata> metadata = await file.GetMetadataAsync(eventContext.CancellationToken);
                    //    Stream content = await file.GetContentAsync(eventContext.CancellationToken);

                    //    ////await DoSomeProcessing(content, metadata);
                    //    content.ToString();

                    //    var fileName = metadata["filename"].GetString(System.Text.Encoding.UTF8);
                    //    var container = metadata["container"].GetString(System.Text.Encoding.UTF8);
                    //    var factor = metadata["factor"].GetString(System.Text.Encoding.UTF8);

                    //    eventContext.ToString();
                    //}
                }
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
                //endpoints.MapGet("/files/{fileId}", async context =>
                //{
                //    context.Response.StatusCode = 501;
                //    await context.Response.WriteAsync("Method Not Implemented");
                //    return;
                //});
            });
        }
    }
}
