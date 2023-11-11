using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using qckdev.AspNetCore.Mvc.Filters.IpSafe;
using qckdev.Extensions.Configuration;
using System;
using System.Text;
using System.Text.Json.Serialization;
using TusWebApplication.Application;
using TusWebApplication.Swagger;
using TusWebApplication.TusAzure;
using TusWebApplication.TusAzure.Authentication;

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
            var jwtTokenConfiguration = Configuration.GetSection("Security").GetSection("Tokens").GetSection("_Default").Get<Settings.JwtTokenConfiguration>();
            var jwtTokenUploadConfiguration = Configuration.GetSection("Security").GetSection("Tokens").GetSection("Upload").Get<TusAzure.Authentication.JwtTokenConfiguration>();
            var ipSafeListSettings = Configuration.GetSection("Security").GetSection("IpSafeList").Get<IpSafeListSettings>();

            services.AddCors(opts => opts.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

            services.Configure<AzureBlobProvider.AzureStorageCredentialsSettings>(options =>
                this.Configuration.GetSection("AzureStorageCredentials").Bind(options)
            );

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    // JWT Token for secured requests.
                    .AddJwtBearer(options =>
                    {
                        options.RequireHttpsMetadata = false;
                        options.SaveToken = true;
                        options.TokenValidationParameters = new TokenValidationParameters()
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtTokenConfiguration.Key)),
                            ValidateAudience = false,
                            ValidateIssuer = false,
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero
                        };
                    },
                    moreOptions =>
                    {
                        moreOptions.TokenLifeTimespan = TimeSpan.FromSeconds(jwtTokenConfiguration.AccessExpireSeconds);
                    })
                    // JWT Token for uploading files.
                    .AddTusJwtBearer(jwtTokenUploadConfiguration)
                ;

            services.Configure<Settings.CredentialsConfiguration>(this.Configuration.GetSection("Security").GetSection("Credentials").Bind);
            services.AddIpSafeFilter(ipSafeListSettings);

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
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSerializedExceptionHandler();

            app.UseTusAzure();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapTusAzure(basePath);
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }

    }
}
