using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TusWebApplication.Swagger
{
    static class DependencyInjection
    {

        const string SWAGGER_V1_ID = "v1";
        const string SWAGGER_V1_NAME = "Default v1";

        static readonly Assembly Assembly = Assembly.GetExecutingAssembly();

        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(SWAGGER_V1_ID, new OpenApiInfo
                {
                    Version = SWAGGER_V1_ID,
                    Title = $"{Assembly.GetName().Name}",
                    Description = string.Join("<br>", GetDescription(Assembly)),
                });
                c.DocumentFilter<CustomDocumentFilter>();
                c.IncludeXmlComments(assembly);
            });
            return services;
        }

        public static IApplicationBuilder UseSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger(setupAction: null);
            app.UseSwaggerUI(c =>
            {
                c.RoutePrefix = "swagger";
                c.DocumentTitle = $"{Assembly.GetName().Name} - {c.DocumentTitle}";
                c.SwaggerEndpoint($"/swagger/{SWAGGER_V1_ID}/swagger.json", SWAGGER_V1_NAME);
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

        /// <summary>
        /// Gets project information.
        /// </summary>
        private static IEnumerable<string> GetDescription(Assembly assembly)
        {
            var result = new List<string>();
            var productId = assembly.GetCustomAttribute<GuidAttribute>()?.Value;
            var description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description;

            result.Add($"<b>Summary</b>");
            if (productId != null)
            {
                result.Add($"Product id: {Guid.Parse(productId)}");
            }
            result.Add($"Product description: {(string.IsNullOrWhiteSpace(description) ? "<null>" : description)}");
            result.Add($"Assembly version: {assembly.GetName().Version}");
            result.Add($"OS Platform: {RuntimeInformation.OSDescription}");
            result.Add($"Target framework: {assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName}");
            return result;
        }

    }
}
