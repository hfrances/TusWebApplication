using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TusWebApplication.Application.Files.Dtos;
using TusWebApplication.Application.Files.Queries;
using TusWebApplication.TusAzure;

namespace TusWebApplication.Application.Files.Handlers
{

    sealed class GetFileByIdQueryHandlers : IRequestHandler<GetFileByIdQuery, FileDto>
    {

        Azure.Storage.Blobs.BlobServiceClient BlobService { get; }

        public GetFileByIdQueryHandlers(IOptions<TusAzure.AzureStorageCredentialSettings> azureOptions)
        {
            this.BlobService = TusAzure.TusAzureHelper.CreateBlobServiceClient(
                azureOptions.Value.AccountName ?? string.Empty,
                azureOptions.Value.AccountKey ?? string.Empty
            );
        }

        public async Task<FileDto> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
        {
            var container = BlobService.GetBlobContainerClient(request.ContainerName);
            BlobClient blob;
            BlobProperties properties;
            IDictionary<string, string> tags;
            IEnumerable<FileVersionDto>? blobVersions = null;
            Uri uri;

            // Obtener el blob. Puede haberse pedido una versión en concreto.
            blob = container.GetBlobClient(request.BlobName);
            if (!string.IsNullOrEmpty(request.Parameters.VersionId))
            {
                blob = blob.WithVersion(request.Parameters.VersionId);
            }
            properties = (await blob.GetPropertiesAsync()).Value;
            tags = (await blob.GetTagsAsync(cancellationToken: cancellationToken)).Value.Tags;
            
            // Generar url.
            if (request.Parameters.GenerateSas && blob.CanGenerateSasUri) // Se ha pedido generar el token de acceso y el blob es privado.
            {
                uri = blob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(12));
            }
            else
            {
                uri = blob.Uri;
            }

            // Obtener versiones.
            if (request.Parameters.LoadVersions && !string.IsNullOrEmpty(properties.VersionId)) // Se ha pedido la lista de versiones y el blob soporta versionado.
            {
                // https://learn.microsoft.com/en-us/azure/storage/blobs/versioning-enable?source=recommendations&tabs=portal#list-blob-versions
                blobVersions = container
                    .GetBlobs(BlobTraits.None, BlobStates.Version, prefix: blob.Name, cancellationToken: cancellationToken)
                        .Where(blob => blob.Name == blob.Name)
                        .OrderByDescending(version => version.VersionId)
                        .Select(value => new FileVersionDto
                        {
                            VersionId = value.VersionId,
                            CreatedOn = value.Properties.CreatedOn
                        });
            }
            return new FileDto
            {
                BlobId = $"{container.Name}/{blob.Name}",
                Name = properties.Metadata.SingleOrDefault(x => x.Key.Equals("filename", StringComparison.OrdinalIgnoreCase)).Value,
                Length = properties.ContentLength,
                Metadata = properties.Metadata,
                Tags = tags,
                Url = uri,
                Checksum = (properties.ContentHash == null) ?
                    null :
                    Convert.ToBase64String(properties.ContentHash),
                CreatedOn = properties.CreatedOn,
                VersionId = properties.VersionId,
                Versions = blobVersions
            };
        }
    }

}
