using Azure.Storage.Blobs;
using System.Threading;
using System.Threading.Tasks;
using System;
using TusWebApplication.TusAzure;

namespace TusWebApplication.Application.Files.Helpers
{
    static class BlobHelper
    {

        public static async Task<TResult> LoadBlob<TResult>(
            AzureBlobProvider.AzureStorageCredentialsSettings azureSettings, IBlobManager tusAzureBlobManager,
            string storeName, string containerName, string blobName, string? versionId,
            Func<BlobStatus?, BlobContainerClient, BlobClient?, CancellationToken, Task<TResult>> action,
            CancellationToken cancellationToken = default)
        {

            if (azureSettings.TryGetValue(storeName, out AzureBlobProvider.AzureStorageCredentialSettings? settings))
            {
                var blobService = AzureBlobProvider.AzureBlobHelper.CreateBlobServiceClient(
                    settings.AccountName, settings.AccountKey
                );
                var container = blobService.GetBlobContainerClient(containerName);
                if (await container.ExistsAsync(cancellationToken))
                {

                    // Obtener el blob.
                    var internalBlob = tusAzureBlobManager.GetBlobStatus(storeName, containerName, blobName);

                    if (internalBlob == null || internalBlob.Status == BlobStatus.UploadStatus.Done)
                    {
                        BlobClient blob;

                        blob = container.GetBlobClient(blobName);
                        if (await blob.ExistsAsync(cancellationToken))
                        {
                            // Obtener versión si se ha especificado (sino estamos cogiendo la última).
                            if (!string.IsNullOrEmpty(versionId))
                            {
                                blob = blob.WithVersion(versionId);
                                if (!await blob.ExistsAsync(cancellationToken))
                                {
                                    throw new Exceptions.BlobVersionNotFoundException();
                                }
                            }
                            return await action(internalBlob, container, blob, cancellationToken);
                        }
                        else
                        {
                            throw new Exceptions.BlobNotFoundException();
                        }
                    }
                    else
                    {
                        return await action(internalBlob, container, null, cancellationToken);
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

        public static Task<TResult> InvokeIfValid<TResult>(BlobStatus? blobStatus, BlobClient? blob, Func<BlobClient, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default)
        {

            if (blobStatus == null || blobStatus.Status == BlobStatus.UploadStatus.Done)
            {
                if (blob == null)
                {
                    throw new qckdev.AspNetCore.HttpHandledException(System.Net.HttpStatusCode.NotFound, "Blob not found");
                }
                else
                {
                    return action(blob, cancellationToken);
                }
            }
            else if (blobStatus.Status == BlobStatus.UploadStatus.Uploading)
            {
                throw new qckdev.AspNetCore.HttpHandledException(System.Net.HttpStatusCode.InternalServerError, "Blob is not ready yet.");
            }
            else if (blobStatus.Status == BlobStatus.UploadStatus.Error)
            {
                throw new qckdev.AspNetCore.HttpHandledException(System.Net.HttpStatusCode.NotFound, "Error uploading blob. It is not available anymore.");
            }
            else
            {
                throw new qckdev.AspNetCore.HttpHandledException(System.Net.HttpStatusCode.NotFound, "Blob not found");
            }
        }

    }
}
