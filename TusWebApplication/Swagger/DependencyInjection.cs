using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using TusWebApplication.Swagger.Filters;

namespace TusWebApplication.Swagger
{
    static class DependencyInjection
    {

        const string SWAGGER_V1_ID = "v1";
        const string SWAGGER_V1_NAME = "Default v1";

        static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(SWAGGER_V1_ID, new OpenApiInfo
                {
                    Version = SWAGGER_V1_ID,
                    Title = $"{Assembly.GetName().Name}"
                });
                c.AddSecurityBearer();
                c.DocumentFilter<DescriptionDocumentFilter>(Assembly);
                c.DocumentFilter<UploadEndpointDocumentFilter>();
                c.CustomSchemaIds(x => x.FullName?.Replace("+", "."));
                c.IncludeXmlComments(Assembly);
            });
            return services;
        }

        static void AddSecurityBearer(this SwaggerGenOptions c)
        {
            var jwtSecurityScheme = new OpenApiSecurityScheme
            {
                BearerFormat = "JWT",
                Name = "JWT Authentication",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                Description = "JWT Authorization header using the Bearer scheme.<br>" +
                              "Enter <b>ONLY</b> your token in the text input (without the prefix 'Bearer').",
                Reference = new OpenApiReference
                {
                    Id = JwtBearerDefaults.AuthenticationScheme,
                    Type = ReferenceType.SecurityScheme
                }
            };

            c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
            c.OperationFilter<SecurityRequirementsOperationFilter>(jwtSecurityScheme);
        }

        public static IApplicationBuilder UseSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger(setupAction: null);
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger";
                c.DocumentTitle = $"{Assembly.GetName().Name} - {c.DocumentTitle}";
                c.SwaggerEndpoint($"{SWAGGER_V1_ID}/swagger.json", SWAGGER_V1_NAME);
                c.DocExpansion(DocExpansion.List); // Endpoints listed.
                c.DefaultModelsExpandDepth(0); // Schema collapsed.
            });

            return app;
        }

        /// <summary>
        /// Sets the comments path for the Swagger JSON and UI
        /// </summary>
        private static void IncludeXmlComments(this SwaggerGenOptions options, Assembly assembly)
        {
            var xmlFile = System.IO.Path.ChangeExtension(assembly.Location, ".xml");
            if (System.IO.File.Exists(xmlFile))
            {
                options.IncludeXmlComments(xmlFile);
            }
        }

    }
}
