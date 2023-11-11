using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using tusdotnet;
using tusdotnet.Models;
using tusdotnet.Models.Concatenation;
using tusdotnet.Models.Configuration;
using tusdotnet.Models.Expiration;
using tusdotnet.Stores;

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
                        if (pair.Value == null)
                        {
                            throw new NullReferenceException($"Settings not found for azure storage '{pair.Key}'.");
                        }
                        else if (pair.Value.CanUploadAsync)
                        {
                            tusAzureStores.Add(pair.Key, new TusAzureStoreQueued(
                                pair.Key,
                                pair.Value.AccountName ?? string.Empty,
                                pair.Value.AccountKey ?? string.Empty,
                                pair.Value.DefaultContainer ?? string.Empty
                            ));
                        }
                        else
                        {
                            tusAzureStores.Add(pair.Key, new TusAzureStore(
                                pair.Key,
                                pair.Value.AccountName ?? string.Empty,
                                pair.Value.AccountKey ?? string.Empty,
                                pair.Value.DefaultContainer ?? string.Empty
                            ));
                        }
                    }
                }
                return tusAzureStores;
            });
            services.AddScoped<IBlobManager, BlobManager>();
            return services;
        }

        public static IApplicationBuilder UseTusAzure(this IApplicationBuilder app)
        {
            var tusAzureStores = app.ApplicationServices.GetService<TusAzureStoreDictionary>();

            if (tusAzureStores != null)
            {
                foreach (var (name, store) in tusAzureStores)
                {
                    app.UseTus(async httpContext =>
                    {
                        var urlPath = CombineUrl(httpContext.Request.PathBase, $"/files/{name}");

                        DefaultTusConfiguration config;

                        config = await TusConfigurationFactory(httpContext, store);
                        config.UrlPath = urlPath;
                        return config;
                    });
                }
            }
            return app;
        }

        [Obsolete("This method is not working for the momment.")] // TODO: Needs more I+D.
        public static IEndpointRouteBuilder MapTusAzure(this IEndpointRouteBuilder endpoints)
        {
            var tusAzureStores = endpoints.ServiceProvider.GetService<TusAzureStoreDictionary>();

            if (tusAzureStores != null)
            {
                foreach (var (name, store) in tusAzureStores)
                {
                    IEndpointConventionBuilder conventionBuilder;

                    conventionBuilder =
                        endpoints.MapTus($"/files/{name}", async context =>
                        {
                            return await TusConfigurationFactory(context, store);
                        });
                }
            }
            return endpoints;
        }

        static string CombineUrl(string basePath, string urlPath)
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

        static Task<DefaultTusConfiguration> TusConfigurationFactory(HttpContext httpContext, tusdotnet.Interfaces.ITusStore tusStore)
        {
            var logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();

            var config = new DefaultTusConfiguration
            {
                Store = tusStore,
                MetadataParsingStrategy = MetadataParsingStrategy.AllowEmptyValues,
                UsePipelinesIfAvailable = true,
                Events = new Events
                {
                    OnAuthorizeAsync = async ctx =>
                    {
                        await AuthorizeAsync(ctx.HttpContext);

                        //bool enableAuthorize = false;

                        //// Note: This event is called even if RequireAuthorization is called on the endpoint.
                        //// In that case this event is not required but can be used as fine-grained authorization control.
                        //// This event can also be used as a "on request started" event to prefetch data or similar.

                        //if (!enableAuthorize)
                        //    return Task.CompletedTask;


                        //if (ctx.HttpContext.User.Identity?.IsAuthenticated != true)
                        //{
                        //    ctx.HttpContext.Response.Headers.Add("WWW-Authenticate", new StringValues("Basic realm=tusdotnet-test-net6.0"));
                        //    ctx.FailRequest(HttpStatusCode.Unauthorized);
                        //    return Task.CompletedTask;
                        //}

                        //if (ctx.HttpContext.User.Identity.Name != "test")
                        //{
                        //    ctx.FailRequest(HttpStatusCode.Forbidden, "'test' is the only allowed user");
                        //    return Task.CompletedTask;
                        //}

                        //// Do other verification on the user; claims, roles, etc.

                        //// Verify different things depending on the intent of the request.
                        //// E.g.:
                        ////   Does the file about to be written belong to this user?
                        ////   Is the current user allowed to create new files or have they reached their quota?
                        ////   etc etc
                        //switch (ctx.Intent)
                        //{
                        //    case IntentType.CreateFile:
                        //        break;
                        //    case IntentType.ConcatenateFiles:
                        //        break;
                        //    case IntentType.WriteFile:
                        //        break;
                        //    case IntentType.DeleteFile:
                        //        break;
                        //    case IntentType.GetFileInfo:
                        //        break;
                        //    case IntentType.GetOptions:
                        //        break;
                        //    default:
                        //        break;
                        //}

                        //return Task.CompletedTask;
                    },

                    OnBeforeCreateAsync = ctx =>
                    {
                        // Partial files are not complete so we do not need to validate
                        // the metadata in our example.
                        return Task.CompletedTask;

                        if (ctx.FileConcatenation is FileConcatPartial)
                        {
                            return Task.CompletedTask;
                        }

                        if (!ctx.Metadata.ContainsKey("name") || ctx.Metadata["name"].HasEmptyValue)
                        {
                            ctx.FailRequest("name metadata must be specified. ");
                        }

                        if (!ctx.Metadata.ContainsKey("contentType") || ctx.Metadata["contentType"].HasEmptyValue)
                        {
                            ctx.FailRequest("contentType metadata must be specified. ");
                        }

                        return Task.CompletedTask;
                    },
                    OnCreateCompleteAsync = ctx =>
                    {
                        logger.LogInformation($"Created file {ctx.FileId} using {ctx.Store.GetType().FullName}");
                        return Task.CompletedTask;
                    },
                    OnBeforeDeleteAsync = ctx =>
                    {
                        // Can the file be deleted? If not call ctx.FailRequest(<message>);
                        return Task.CompletedTask;
                    },
                    OnDeleteCompleteAsync = ctx =>
                    {
                        logger.LogInformation($"Deleted file {ctx.FileId} using {ctx.Store.GetType().FullName}");
                        return Task.CompletedTask;
                    },
                    OnFileCompleteAsync = ctx =>
                    {
                        logger.LogInformation($"Upload of {ctx.FileId} completed using {ctx.Store.GetType().FullName}");
                        // If the store implements ITusReadableStore one could access the completed file here.
                        // The default TusDiskStore implements this interface:
                        //var file = await ctx.GetFileAsync();
                        return Task.CompletedTask;
                    }
                },
                // Set an expiration time where incomplete files can no longer be updated.
                // This value can either be absolute or sliding.
                // Absolute expiration will be saved per file on create
                // Sliding expiration will be saved per file on create and updated on each patch/update.
                Expiration = new AbsoluteExpiration(TimeSpan.FromMinutes(5))
            };

            return Task.FromResult(config);
        }

        static async Task AuthorizeAsync(HttpContext context)
        {
            var schemaName = TusAzure.Authentication.Constants.UPLOAD_FILE_SCHEMA;
            var authenticationResult = await context.AuthenticateAsync(schemaName);

            if (authenticationResult.Succeeded)
            {
                var authorizationService = context.RequestServices.GetRequiredService<IAuthorizationService>();
                var user = authenticationResult.Principal;
                var policy =
                    new AuthorizationPolicyBuilder(schemaName)
                        .RequireAuthenticatedUser()
                        .Build();
                var authorizationResult = await authorizationService.AuthorizeAsync(user, policy);

                if (authorizationResult.Succeeded)
                {
                    // Success.
                    await Task.CompletedTask;
                }
                else
                {
                    // El usuario no puede realizar la operación.
                    throw new qckdev.AspNetCore.HttpHandledException(System.Net.HttpStatusCode.Unauthorized, null);
                }
            }
            else
            {
                // El usuario no está autenficiado.
                throw new qckdev.AspNetCore.HttpHandledException(System.Net.HttpStatusCode.Unauthorized, authenticationResult.Failure?.Message);
            }

        }

    }
}
