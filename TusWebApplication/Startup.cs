using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using qckdev.Extensions.Configuration;
using System;
using System.Text.Json.Serialization;
using TusWebApplication.Application;
using TusWebApplication.Swagger;
using TusWebApplication.TusAzure;

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
            services.AddCors(opts => opts.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            services.Configure<AzureBlobProvider.AzureStorageCredentialsSettings>(options =>
                this.Configuration.GetSection("AzureStorageCredentials").Bind(options)
            );

            services
                .AddApplication()
                .AddTusAzure();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                });

            services.AddSwagger();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var basePath = this.Configuration.GetSection("BasePath")?.Value ?? "/";

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

            app.UseTusAzure(basePath);

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
