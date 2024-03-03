using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using TusWebApplication.Application.Files.Helpers;
using TusWebApplication.Application.Files.Queries;
using TusWebApplication.AzureBlobProvider;

namespace TusWebApplication.Application.Files.Handlers
{

    sealed class DownloadFileQueryHandler : IRequestHandler<DownloadFileQuery, IDownloadableFileInfo>
    {

        AzureBlobProvider.AzureStorageCredentialsSettings AzureSettings { get; }
        AzureBlobProvider.AzureBlobFileProvider AzureBlobFileProvider { get; }
        TusAzure.IBlobManager TusAzureBlobManager { get; }
        ILogger Logger { get; }

        public DownloadFileQueryHandler(
            IOptions<AzureBlobProvider.AzureStorageCredentialsSettings> azureOptions,
            AzureBlobProvider.AzureBlobFileProvider azureBlobFileProvider,
            TusAzure.IBlobManager tusAzureBlobManager, ILogger<DownloadFileQueryHandler> logger)
        {
            this.AzureSettings = azureOptions.Value;
            this.AzureBlobFileProvider = azureBlobFileProvider;
            this.TusAzureBlobManager = tusAzureBlobManager;
            this.Logger = logger;
        }

        public Task<IDownloadableFileInfo> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
            => BlobHelper.LoadBlob(
                AzureSettings, TusAzureBlobManager,
                request.StoreName, request.ContainerName, request.BlobName, request.Parameters?.VersionId,
                (blobStatus, container, blob, cancellationToken) =>
                    BlobHelper.InvokeIfValid(blobStatus, blob, 
                        async (blob, _) => {
                            var properties = (await blob.GetPropertiesAsync(cancellationToken: cancellationToken)).Value;
                            var containerAccessPolicy = (await container.GetAccessPolicyAsync()).Value.BlobPublicAccess;
                            var useSas = (blob.CanGenerateSasUri && containerAccessPolicy == Azure.Storage.Blobs.Models.PublicAccessType.None);
                            string subPath;

                            Logger.LogInformation($"Requested SAS token for {request.StoreName}/{request.ContainerName}/{request.BlobName}. Request: {request.Parameters?.Se}; Current: {DateTimeOffset.UtcNow}; Difference: {request.Parameters?.Se - DateTimeOffset.UtcNow}");
                            SasHelper.ValidateSasHash(request.Parameters?.Sv, request.Parameters?.Se, request.Parameters?.Sig, blob, properties, useSas);
                            subPath = $"{request.StoreName}/{request.ContainerName}/{request.BlobName}";
                            if (!string.IsNullOrWhiteSpace(request.Parameters?.VersionId))
                            {
                                subPath += $"?versionId={request.Parameters.VersionId}";
                            }
                            return AzureBlobFileProvider.GetFileInfo(subPath);
                        }, 
                    cancellationToken), 
                cancellationToken);

    }
}