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
using TusWebApplication.Application.Files.Helpers;
using System.ComponentModel;

namespace TusWebApplication.Application.Files.Handlers
{

    sealed class GetFileByIdQueryHandlers : IRequestHandler<GetFileByIdQuery, FileDto>
    {

        AzureBlobProvider.AzureStorageCredentialsSettings AzureSettings { get; }
        TusAzure.IBlobManager TusAzureBlobManager { get; }

        public GetFileByIdQueryHandlers(
            IOptions<AzureBlobProvider.AzureStorageCredentialsSettings> azureOptions,
            TusAzure.IBlobManager tusAzureBlobManager)
        {
            this.AzureSettings = azureOptions.Value;
            this.TusAzureBlobManager = tusAzureBlobManager;
        }

        public Task<FileDto> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
            => BlobHelper.LoadBlob(
                AzureSettings, TusAzureBlobManager,
                request.StoreName, request.ContainerName, request.BlobName, request.Parameters?.VersionId,
                async (internalBlob, container, blob, cancellationToken) =>
                {

                    if (blob == null || container == null)
                    {
                        if (internalBlob == null)
                        {
                            // Condición controlada en LoadBlob.
                            // Aquí o llega el blob con valor, o llega el internalBlob con valor, pero no pueden llegar ambos sin valor.
                            throw new NullReferenceException();
                        }
                        else
                        {
                            return new FileDto
                            {
                                BlobId = internalBlob.BlobId,
                                Name = internalBlob.Name,
                                Length = internalBlob.Length,
                                Status = (FileDto.UploadStatus)internalBlob.Status,
                                UploadPercentage = internalBlob.RemotePercentage
                            };
                        }
                    }
                    else
                    {
                        BlobProperties properties;
                        IDictionary<string, string> tags;
                        IEnumerable<FileVersionDto>? blobVersions = null;
                        Uri uri;

                        properties = (await blob.GetPropertiesAsync(cancellationToken: cancellationToken)).Value;
                        tags = (await blob.GetTagsAsync(cancellationToken: cancellationToken)).Value.Tags;

                        // Generar url.
                        uri = blob.Uri;

                        // Obtener versiones.
                        if (request.Parameters != null && request.Parameters.LoadVersions && !string.IsNullOrEmpty(properties.VersionId)) // Se ha pedido la lista de versiones y el blob soporta versionado.
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
                },
                cancellationToken
            );

    }

}
