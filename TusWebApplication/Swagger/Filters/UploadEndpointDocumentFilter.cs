using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;

namespace TusWebApplication.Swagger.Filters
{

    /// <remarks>
    /// https://medium.com/@niteshsinghal85/documenting-additional-api-endpoints-in-swagger-in-asp-net-core-59da9c84e4ba
    /// </remarks>
    sealed class UploadEndpointDocumentFilter : IDocumentFilter
    {

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            AddPath(
                swaggerDoc,
                "Files",
                "/files/{store}",
                OperationType.Post,
                "Uploads a file using TUS technology. Requires an authentication bearer token created with 'request-upload' endpoint.",
                new[] {
                    ("200", "Success"),
                    ("201", "Created"),
                    ("204", "No Content"),
                    ("400", "Bad Request"),
                    ("460", "Checksum Mismatch"),
                },
                new Uri("https://tus.io/protocols/resumable-upload")
            );
        }

        private static void AddPath(
            OpenApiDocument swaggerDoc,
            string tag,
            string key,
            OperationType operationType,
            string? summary = null,
            IEnumerable<(string Key, string Description)>? responses = null,
            Uri? externalUrl = null
        )
        {
            // define operation
            var operation = new OpenApiOperation
            {
                Tags = new[] { new OpenApiTag { Name = tag } },
                Summary = summary,
                ExternalDocs = externalUrl == null ? null : new OpenApiExternalDocs { Url = externalUrl },
                Callbacks = null
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
