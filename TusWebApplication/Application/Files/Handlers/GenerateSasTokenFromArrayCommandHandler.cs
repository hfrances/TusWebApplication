using MediatR;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using TusWebApplication.Application.Files.Commands;
using TusWebApplication.Application.Files.Helpers;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.WebUtilities;
using TusWebApplication.Application.Files.Dtos;

namespace TusWebApplication.Application.Files.Handlers
{
    sealed class GenerateSasTokenFromArrayCommandHandler : IRequestHandler<GenerateSasTokenFromArrayCommand, IEnumerable<TokenSasDto>>
    {

        AzureBlobProvider.AzureStorageCredentialsSettings AzureSettings { get; }
        TusAzure.IBlobManager TusAzureBlobManager { get; }

        public GenerateSasTokenFromArrayCommandHandler(
            IOptions<AzureBlobProvider.AzureStorageCredentialsSettings> azureOptions,
            TusAzure.IBlobManager tusAzureBlobManager)
        {
            this.AzureSettings = azureOptions.Value;
            this.TusAzureBlobManager = tusAzureBlobManager;
        }

        public async Task<IEnumerable<TokenSasDto>> Handle(GenerateSasTokenFromArrayCommand request, CancellationToken cancellationToken)
        {
            var blobs = request.Body.Blobs.ToList();

            if (blobs.Count == 0)
            {
                return Array.Empty<TokenSasDto>();
            }
            else if (blobs.Count == 1)
            {
                // Un único blob: comprobación completa (contenedor + blob), igual que el endpoint individual.
                return new[] { await GenerateSingleTokenAsync(request, blobs[0], cancellationToken) };
            }
            else
            {
                // Varios blobs: solo se comprueba la existencia del contenedor (una vez, arriba). No se comprueba
                // cada blob individualmente para evitar N llamadas remotas; el token se genera de forma optimista.
                return await BlobHelper.LoadContainer(
                    AzureSettings, TusAzureBlobManager,
                    request.StoreName, request.ContainerName,
                    async (container, cancellationToken) =>
                    {
                        var result = new List<TokenSasDto>();
                        var containerAccessPolicy = (await container.GetAccessPolicyAsync()).Value.BlobPublicAccess;
                        var useSas = (containerAccessPolicy == Azure.Storage.Blobs.Models.PublicAccessType.None);

                        foreach (var item in blobs)
                        {
                            string? token;

                            if (useSas)
                            {
                                var internalBlob = TusAzureBlobManager.GetBlobStatus(request.StoreName, container.Name, item.BlobName);

                                if (internalBlob != null && internalBlob.Status != TusAzure.BlobStatus.UploadStatus.Done)
                                {
                                    // Subida en curso, con error, o estado desconocido: no se genera token.
                                    token = null;
                                }
                                else
                                {
                                    var blob = container.GetBlobClient(item.BlobName);

                                    if (!string.IsNullOrEmpty(item.VersionId))
                                    {
                                        blob = blob.WithVersion(item.VersionId);
                                    }
                                    token = SasHelper.GenerateSasString(request.Body.ExpiresOn, blob, item.VersionId);
                                }
                            }
                            else
                            {
                                token = "p=true";
                            }
                            result.Add(new TokenSasDto
                            {
                                StoreName = request.StoreName,
                                ContainerName = request.ContainerName,
                                BlobName = item.BlobName,
                                Version = item.VersionId,
                                TokenSas = token
                            });
                        }
                        return result;

                    }, cancellationToken);
            }
        }

        async Task<TokenSasDto> GenerateSingleTokenAsync(GenerateSasTokenFromArrayCommand request, GenerateSasTokenFromArrayCommand.BlobInfo item, CancellationToken cancellationToken)
        {
            var token = await BlobHelper.GenerateSasTokenAsync(
                AzureSettings, TusAzureBlobManager,
                request.StoreName, request.ContainerName, item.BlobName, item.VersionId,
                request.Body.ExpiresOn, cancellationToken);

            return new TokenSasDto
            {
                StoreName = request.StoreName,
                ContainerName = request.ContainerName,
                BlobName = item.BlobName,
                Version = item.VersionId,
                TokenSas = token
            };
        }

    }
}
