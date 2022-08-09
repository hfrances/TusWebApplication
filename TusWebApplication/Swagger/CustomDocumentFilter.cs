using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace TusWebApplication.Swagger
{

    /// <remarks>
    /// https://medium.com/@niteshsinghal85/documenting-additional-api-endpoints-in-swagger-in-asp-net-core-59da9c84e4ba
    /// </remarks>
    sealed class CustomDocumentFilter : IDocumentFilter
    {

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            AddPath(swaggerDoc, "File", "/file", OperationType.Post, "Uploads a file using TUS technology", new[] { ("200", "Success") });
            AddPath(swaggerDoc, "File", "/file/{fileId}", OperationType.Get, "Retrieves a file", new[] { ("200", "Success") });
        }

        private static void AddPath(OpenApiDocument swaggerDoc, string tag, string key, OperationType operationType, string? summary = null, IEnumerable<(string Key, string Description)>? responses = null)
        {
            // define operation
            var operation = new OpenApiOperation
            {
                Tags = new[] { new OpenApiTag { Name = tag } },
                Summary = summary
            };

            if (responses != null)
            {
                foreach (var response in responses)
                {
                    operation.Responses.Add(response.Key, new OpenApiResponse()
                    {
                        Description = response.Description,
                    });
                }
            }

            // create path item
            var pathItem = new OpenApiPathItem();

            // add operation to the path
            pathItem.AddOperation(operationType, operation);

            // finally add the path to document
            swaggerDoc?.Paths.Add(key, pathItem);
        }

    }
}
