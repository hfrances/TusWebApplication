using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TusWebApplication.TusAzure.Authentication
{
    static class TusAuthenticationHelper
    {
        public static async Task<System.Security.Claims.ClaimsPrincipal?> GetUser(HttpContext context, string schemaName)
        {
            var authenticationResult = await context.AuthenticateAsync(schemaName);

            if (authenticationResult.Succeeded)
            {
                return authenticationResult.Principal;
            }
            else
            {
                // El usuario no está autenficiado.
                throw new qckdev.AspNetCore.HttpHandledException(System.Net.HttpStatusCode.Unauthorized, authenticationResult.Failure?.Message);
            }

        }

        public static async Task AuthorizeAsync(HttpContext context, string schemaName)
        {
            var authorizationService = context.RequestServices.GetRequiredService<IAuthorizationService>();
            var user = await GetUser(context, schemaName);
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

        public static IEnumerable<System.Security.Claims.Claim> CreateClaims(UploadProperties properties)
        {
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim("container", properties.Container),
                new System.Security.Claims.Claim("file-name", properties.FileName),
                new System.Security.Claims.Claim("blob", properties.Blob ?? ""),
                new System.Security.Claims.Claim("replace", properties.Replace.ToString()),
                new System.Security.Claims.Claim("size", properties.Size.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                new System.Security.Claims.Claim("hash", properties.Hash ?? ""),
                new System.Security.Claims.Claim("use-async", properties.UseQueueAsync.ToString()),
                new System.Security.Claims.Claim("expired", properties.FirstRequestExpired.ToString("O", System.Globalization.CultureInfo.InvariantCulture))
            };
            return claims;
        }

        public static UploadProperties ParseClaims(IEnumerable<System.Security.Claims.Claim> claims)
        {
            var properties = new UploadProperties
            {
                Container = claims.Single(x => x.Type == "container").Value,
                FileName = claims.Single(x => x.Type == "file-name").Value,
                Blob = claims.Single(x => x.Type == "blob").Value,
                Replace = bool.Parse(claims.Single(x => x.Type == "replace").Value),
                Size = long.Parse(claims.Single(x => x.Type == "size").Value, System.Globalization.CultureInfo.InvariantCulture),
                Hash = claims.Single(x => x.Type == "hash").Value,
                UseQueueAsync = bool.Parse(claims.Single(x => x.Type == "use-async").Value),
                FirstRequestExpired = DateTimeOffset.Parse(claims.Single(x => x.Type == "expired").Value, System.Globalization.CultureInfo.InvariantCulture)
            };
            return properties;
        }

    }
}
