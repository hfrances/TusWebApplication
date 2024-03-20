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
            var result = new List<TokenSasDto>();

            foreach (var item in request.Body.Blobs)
            {
                string? token;

                try
                {
                    token =
                        await BlobHelper.LoadBlob(
                            AzureSettings, TusAzureBlobManager,
                            request.StoreName, request.ContainerName, item.BlobName, item.VersionId,
                            (internalBlob, container, blob, cancellationToken) =>
                                BlobHelper.InvokeIfValid(internalBlob, blob,
                                    async (blob, cancellationToken) =>
                                    {
                                        var properties = (await blob.GetPropertiesAsync(cancellationToken: cancellationToken)).Value;
                                        var tokenSas = SasHelper.GenerateSasString(request.Body.ExpiresOn, blob, properties, item.VersionId);

                                        return tokenSas;
                                    }, cancellationToken),
                            cancellationToken);
                }
                catch (qckdev.AspNetCore.HttpHandledException ex) when (ex.ErrorCode == System.Net.HttpStatusCode.NotFound)
                {
                    token = null;
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
        }
        
    }
}
