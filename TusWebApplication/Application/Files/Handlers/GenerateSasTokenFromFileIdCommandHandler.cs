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

namespace TusWebApplication.Application.Files.Handlers
{
    sealed class GenerateSasTokenFromFileIdCommandHandler : IRequestHandler<GenerateSasTokenFromFileIdCommand, string>
    {

        AzureBlobProvider.AzureStorageCredentialsSettings AzureSettings { get; }
        TusAzure.IBlobManager TusAzureBlobManager { get; }

        public GenerateSasTokenFromFileIdCommandHandler(
            IOptions<AzureBlobProvider.AzureStorageCredentialsSettings> azureOptions,
            TusAzure.IBlobManager tusAzureBlobManager)
        {
            this.AzureSettings = azureOptions.Value;
            this.TusAzureBlobManager = tusAzureBlobManager;
        }

        public Task<string> Handle(GenerateSasTokenFromFileIdCommand request, CancellationToken cancellationToken)
            => BlobHelper.LoadBlob(
                AzureSettings, TusAzureBlobManager,
                request.StoreName, request.ContainerName, request.BlobName, request.Parameters?.VersionId,
                (internalBlob, container, blob, cancellationToken) =>
                    BlobHelper.InvokeIfValid(internalBlob, blob,
                        async (blob, cancellationToken) =>
                        {
                            var properties = (await blob.GetPropertiesAsync(cancellationToken: cancellationToken)).Value;
                            var token = SasHelper.GenerateSasHash(request.Body.ExpiresOn, blob, properties);
                            var query = new Dictionary<string, string?>();
                            
                            if (request.Parameters?.VersionId != null)
                            {
                                query.Add("versionId", request.Parameters.VersionId);
                            }
                            query.Add("sv", "1");
                            query.Add("se", request.Body.ExpiresOn.ToString("O"));
                            query.Add("sig", token);
                            return QueryHelpers.AddQueryString("", query);

                        }, cancellationToken),
                cancellationToken);

    }
}
