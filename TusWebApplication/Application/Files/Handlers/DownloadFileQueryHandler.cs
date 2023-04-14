using Azure.Storage.Blobs;
using MediatR;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Writers;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using TusWebApplication.Application.Files.Dtos;
using TusWebApplication.Application.Files.Helpers;
using TusWebApplication.Application.Files.Queries;

namespace TusWebApplication.Application.Files.Handlers
{

    sealed class DownloadFileQueryHandler : IRequestHandler<DownloadFileQuery, Microsoft.Extensions.FileProviders.IFileInfo>
    {

        AzureBlobProvider.AzureStorageCredentialsSettings AzureSettings { get; }
        AzureBlobProvider.AzureBlobFileProvider AzureBlobFileProvider { get; }
        TusAzure.IBlobManager TusAzureBlobManager { get; }

        public DownloadFileQueryHandler(
            IOptions<AzureBlobProvider.AzureStorageCredentialsSettings> azureOptions,
            AzureBlobProvider.AzureBlobFileProvider azureBlobFileProvider,
            TusAzure.IBlobManager tusAzureBlobManager)
        {
            this.AzureSettings = azureOptions.Value;
            this.AzureBlobFileProvider = azureBlobFileProvider;
            this.TusAzureBlobManager = tusAzureBlobManager;
        }

        public Task<IFileInfo> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
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