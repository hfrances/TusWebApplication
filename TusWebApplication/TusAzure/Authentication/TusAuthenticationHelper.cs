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

        const string CLAIMS_CONTAINER = "container";
        const string CLAIMS_FILENAME = "file-name";
        const string CLAIMS_BLOB = "blob";
        const string CLAIMS_BLOBID = "blobId";
        const string CLAIMS_CONTENTTYPE = "content-type";
        const string CLAIMS_CONTENTLANGUAGE = "content-language";
        const string CLAIMS_REPLACE = "replace";
        const string CLAIMS_SIZE = "size";
        const string CLAIMS_HASH = "hash";
        const string CLAIMS_USEASYNC = "use-async";
        const string CLAIMS_EXPIRED = "expired";


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
            // Mandatory claims.
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(CLAIMS_CONTAINER, properties.Container),
                new System.Security.Claims.Claim(CLAIMS_FILENAME, properties.FileName),
                new System.Security.Claims.Claim(CLAIMS_BLOB, properties.Blob),
                new System.Security.Claims.Claim(CLAIMS_BLOBID, properties.BlobId),
                new System.Security.Claims.Claim(CLAIMS_CONTENTTYPE, properties.ContentType ?? ""),
                new System.Security.Claims.Claim(CLAIMS_REPLACE, properties.Replace.ToString()),
                new System.Security.Claims.Claim(CLAIMS_SIZE, properties.Size.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                new System.Security.Claims.Claim(CLAIMS_HASH, properties.Hash ?? ""),
                new System.Security.Claims.Claim(CLAIMS_USEASYNC, properties.UseQueueAsync.ToString()),
                new System.Security.Claims.Claim(CLAIMS_EXPIRED, properties.FirstRequestExpired.ToString("O", System.Globalization.CultureInfo.InvariantCulture))
            };

            // Optional claims.
            if (properties.ContentLanguage != null)
            {
                claims.Add(new System.Security.Claims.Claim(CLAIMS_CONTENTLANGUAGE, properties.ContentLanguage));
            }
            return claims;
        }

        public static UploadProperties ParseClaims(IEnumerable<System.Security.Claims.Claim> claims)
        {
            var properties = new UploadProperties
            {
                Container = claims.Single(x => x.Type == CLAIMS_CONTAINER).Value,
                FileName = claims.Single(x => x.Type == CLAIMS_FILENAME).Value,
                Blob = claims.Single(x => x.Type == CLAIMS_BLOB).Value,
                BlobId = claims.Single(x => x.Type == CLAIMS_BLOBID).Value,
                ContentType = claims.Single(x => x.Type == CLAIMS_CONTENTTYPE).Value,
                ContentLanguage = claims.SingleOrDefault(x => x.Type == CLAIMS_CONTENTLANGUAGE)?.Value,
                Replace = bool.Parse(claims.Single(x => x.Type == CLAIMS_REPLACE).Value),
                Size = long.Parse(claims.Single(x => x.Type == CLAIMS_SIZE).Value, System.Globalization.CultureInfo.InvariantCulture),
                Hash = claims.Single(x => x.Type == CLAIMS_HASH).Value,
                UseQueueAsync = bool.Parse(claims.Single(x => x.Type == CLAIMS_USEASYNC).Value),
                FirstRequestExpired = DateTimeOffset.Parse(claims.Single(x => x.Type == CLAIMS_EXPIRED).Value, System.Globalization.CultureInfo.InvariantCulture)
            };
            return properties;
        }

    }
}
