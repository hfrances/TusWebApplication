using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TusWebApplication.Swagger.Filters
{

    /// <remarks>
    /// https://stackoverflow.com/questions/59158352/swagger-ui-authentication-only-for-some-endpoints
    /// </remarks>
    sealed class SecurityRequirementsOperationFilter : IOperationFilter
    {

        OpenApiSecurityScheme Scheme { get; }

        public SecurityRequirementsOperationFilter(OpenApiSecurityScheme scheme)
        {
            this.Scheme = scheme;
        }

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            if (!context.MethodInfo.GetCustomAttributes(true).Any(x => x is AllowAnonymousAttribute) &&
                !(context.MethodInfo.DeclaringType?.GetCustomAttributes(true).Any(x => x is AllowAnonymousAttribute) ?? false))
            {
                operation.Security = new List<OpenApiSecurityRequirement>
                {
                    new OpenApiSecurityRequirement
                    {
                        { this.Scheme, Array.Empty<string>() }
                    }
                };
            }
        }
    }
}
