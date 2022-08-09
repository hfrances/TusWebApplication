using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Reflection;

namespace TusWebApplication.Swagger
{
    static class DependencyInjection
    {

        const string SWAGGER_TITLE = "TusWebApplication Api";
        const string SWAGGER_V1_ID = "v1";
        const string SWAGGER_V1_NAME = "Default v1";

        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(SWAGGER_V1_ID, new OpenApiInfo
                {
                    Version = SWAGGER_V1_ID,
                    Title = SWAGGER_TITLE,
                    Description = $"Provides demo actions.\n Assembly version: {assembly.GetName().Version}",
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
                c.DocumentTitle = $"{SWAGGER_TITLE} - {c.DocumentTitle}";
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

    }
}
