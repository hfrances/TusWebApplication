using Azure.Storage.Blobs;
using System.Threading;
using System.Threading.Tasks;
using System;
using TusWebApplication.TusAzure;

namespace TusWebApplication.Application.Files.Helpers
{
    static class BlobHelper
    {

        public static async Task<TResult> LoadContainer<TResult>(
            AzureBlobProvider.AzureStorageCredentialsSettings azureSettings, IBlobManager tusAzureBlobManager,
            string storeName, string containerName,
            Func<BlobContainerClient, CancellationToken, Task<TResult>> action,
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
                    return await action(container, cancellationToken);
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
                    return await LoadBlob(tusAzureBlobManager, storeName, container, blobName, versionId, action, cancellationToken);
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

        public static async Task<TResult> LoadBlob<TResult>(
            IBlobManager tusAzureBlobManager,
            string storeName, BlobContainerClient container, string blobName, string? versionId,
            Func<BlobStatus?, BlobContainerClient, BlobClient?, CancellationToken, Task<TResult>> action,
            CancellationToken cancellationToken = default)
        {
            var internalBlob = await tusAzureBlobManager.GetBlobStatusAsync(storeName, container.Name, blobName, cancellationToken);

            // Obtener el blob.
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


        public static Task<TResult> InvokeIfValid<TResult>(BlobStatus? blobStatus, BlobClient? blob, Func<BlobClient, CancellationToken, Task<TResult>> action, CancellationToken cancellationToken = default)
        {

            if (blobStatus == null || blobStatus.Status == BlobStatus.UploadStatus.Done)
            {
                if (blob == null)
                {
                    throw new Exceptions.BlobNotFoundException();
                }
                else
                {
                    return action(blob, cancellationToken);
                }
            }
            else if (blobStatus.Status == BlobStatus.UploadStatus.Uploading)
            {
                throw new Exceptions.BlobNotReadyException();
            }
            else if (blobStatus.Status == BlobStatus.UploadStatus.Error)
            {
                throw new qckdev.AspNetCore.HttpHandledException(System.Net.HttpStatusCode.Gone, "Error uploading blob. It is not available anymore.");
            }
            else
            {
                throw new Exceptions.BlobNotFoundException();
            }
        }

        /// <summary>
        /// Comprueba la existencia del contenedor y del blob (vía completa) y genera un token SAS para él,
        /// usando las propiedades del blob (hash "seguro"). Lanza 404 (BlobNotFoundException, etc.) si no existe.
        /// </summary>
        public static Task<string> GenerateSasTokenAsync(
            AzureBlobProvider.AzureStorageCredentialsSettings azureSettings, IBlobManager tusAzureBlobManager,
            string storeName, string containerName, string blobName, string? versionId, DateTimeOffset expiresOn,
            CancellationToken cancellationToken = default)
        {
            return LoadBlob(
                azureSettings, tusAzureBlobManager,
                storeName, containerName, blobName, versionId,
                (internalBlob, container, blob, cancellationToken) =>
                    InvokeIfValid(internalBlob, blob,
                        async (blob, cancellationToken) =>
                        {
                            var containerAccessPolicy = (await container.GetAccessPolicyAsync()).Value.BlobPublicAccess;
                            var useSas = (containerAccessPolicy == Azure.Storage.Blobs.Models.PublicAccessType.None);
                            string tokenSas = "p=true";

                            if (useSas)
                            {
                                var properties = (await blob.GetPropertiesAsync(cancellationToken: cancellationToken)).Value;
                                tokenSas = SasHelper.GenerateSasString(expiresOn, blob, versionId, properties);
                            }
                            return tokenSas;
                        }, cancellationToken),
                cancellationToken);
        }

    }
}
