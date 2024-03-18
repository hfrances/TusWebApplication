using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using TusWebApplication.Application.Files.Commands;

namespace TusWebApplication.Application.Files.Handlers
{
    public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand>
    {

        AzureBlobProvider.AzureStorageCredentialsSettings AzureSettings { get; }

        public DeleteFileCommandHandler(IOptions<AzureBlobProvider.AzureStorageCredentialsSettings> azureOptions)
        {
            this.AzureSettings = azureOptions.Value;
        }

        public async Task<Unit> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
        {

            try
            {
                if (AzureSettings.TryGetValue(request.StoreName, out AzureBlobProvider.AzureStorageCredentialSettings? settings))
                {
                    var blobService = AzureBlobProvider.AzureBlobHelper.CreateBlobServiceClient(
                        settings.AccountName, settings.AccountKey
                    );
                    var container = blobService.GetBlobContainerClient(request.ContainerName);

                    if (await container.ExistsAsync(cancellationToken))
                    {
                        BlobClient blob;

                        blob = container.GetBlobClient(request.BlobName);
                        if (!string.IsNullOrWhiteSpace(request.Parameters.VersionId))
                        {
                            blob = blob.WithVersion(request.Parameters.VersionId);
                        }
                        if (await blob.ExistsAsync(cancellationToken))
                        {
                            Azure.Response response;

                            response = await blob.DeleteAsync(DeleteSnapshotsOption.IncludeSnapshots);
                            if (response.IsError)
                            {
                                throw new Exception(response.ReasonPhrase);
                            }
                            else
                            {
                                return Unit.Value;
                            }
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
            catch (qckdev.AspNetCore.HttpHandledException ex)
            {
                throw new Exceptions.ImportBlobException(ex);
            }
            catch (Exception ex)
            {
                throw new Exceptions.ImportBlobException(ex);
            }
        }

    }
}
