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

namespace TusWebApplication.Application.Files.Handlers
{

    sealed class GetFileByIdQueryHandlers : IRequestHandler<GetFileByIdQuery, FileDto>
    {

        AzureBlobProvider.AzureStorageCredentialsSettings AzureSettings { get; }

        public GetFileByIdQueryHandlers(IOptions<AzureBlobProvider.AzureStorageCredentialsSettings> azureOptions)
        {
            this.AzureSettings = azureOptions.Value;
        }

        public async Task<FileDto> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
        {

            if (AzureSettings.TryGetValue(request.StoreName, out AzureBlobProvider.AzureStorageCredentialSettings? settings))
            {
                var blobService = AzureBlobProvider.AzureBlobHelper.CreateBlobServiceClient(
                    settings.AccountName ?? string.Empty,
                    settings.AccountKey ?? string.Empty
                );
                var container = blobService.GetBlobContainerClient(request.ContainerName);

                if (await container.ExistsAsync(cancellationToken))
                {
                    BlobClient blob;
                    BlobProperties properties;
                    IDictionary<string, string> tags;
                    IEnumerable<FileVersionDto>? blobVersions = null;
                    Uri uri;

                    // Obtener el blob.
                    blob = container.GetBlobClient(request.BlobName);
                    if (await blob.ExistsAsync(cancellationToken))
                    {
                        // Obtener versión si se ha especificado (sino estamos cogiendo la última).
                        if (!string.IsNullOrEmpty(request.Parameters.VersionId))
                        {
                            blob = blob.WithVersion(request.Parameters.VersionId);
                            if (!await blob.ExistsAsync(cancellationToken))
                            {
                                throw new Exceptions.BlobVersionNotFoundException();
                            }
                        }
                        properties = (await blob.GetPropertiesAsync(cancellationToken: cancellationToken)).Value;
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
                    else
                    {
                        throw new Exceptions.BlobNotFoundException();
                    }
                }
                else
                {
                    throw new Exceptions.ContainerNotFoundException();
                }
            }
            else
            {
                throw new Exceptions.BlobStorageNotFoundException();
            }
        }
    }

}
