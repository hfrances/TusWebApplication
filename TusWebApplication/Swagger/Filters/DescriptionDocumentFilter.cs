using Microsoft.AspNetCore.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TusWebApplication.Swagger.Filters
{
    sealed class DescriptionDocumentFilter : IDocumentFilter
    {

        Assembly Assembly { get; }
        IWebHostEnvironment Environment { get; }

        public DescriptionDocumentFilter(Assembly assembly, IWebHostEnvironment environment)
        {
            this.Assembly = assembly;
            this.Environment = environment;
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            swaggerDoc.Info.Description = string.Join("<br>", GetDescription(Assembly, Environment));
        }

        /// <summary>
        /// Gets project information.
        /// </summary>
        private static IEnumerable<string> GetDescription(Assembly assembly, IWebHostEnvironment environment)
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
            result.Add($"Environment: {environment?.EnvironmentName ?? "<null>"}");
            result.Add($"OS Platform: {RuntimeInformation.OSDescription}");
            result.Add($"Target framework: {assembly.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName}");
            return result;
        }

    }
}
