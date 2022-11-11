using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using TusWebApplication.Application.Files.Commands;

namespace TusWebApplication.Application.Files.Handlers
{

    sealed class RenameFileCommandHandler : IRequestHandler<RenameFileCommand>
    {

        Azure.Storage.Blobs.BlobServiceClient BlobService { get; }

        public RenameFileCommandHandler(IOptions<TusAzure.AzureStorageCredentialSettings> azureOptions)
        {
            this.BlobService = TusAzure.TusAzureHelper.CreateBlobServiceClient(
                azureOptions.Value.AccountName ?? string.Empty,
                azureOptions.Value.AccountKey ?? string.Empty
            );
        }

        public async Task<Unit> Handle(RenameFileCommand request, CancellationToken cancellationToken)
        {
            var container = BlobService.GetBlobContainerClient(request.ContainerName);

            if (request.Body == null)
            {
                throw new NullReferenceException("Body not found.");
            }
            else if (string.IsNullOrWhiteSpace(request.Body.BlobName))
            {
                throw new NullReferenceException("New blob name not specified.");
            }
            else
            {
                if (await container.ExistsAsync(cancellationToken))
                {
                    BlobClient blob, newBlob;

                    blob = container.GetBlobClient(request.BlobName);
                    if (await blob.ExistsAsync(cancellationToken))
                    {
                        CopyFromUriOperation copy;
                        Azure.Response<long> response;
                        var properties = (await blob.GetPropertiesAsync(cancellationToken: cancellationToken)).Value;
                        var secureUri = blob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTime.UtcNow.AddHours(1));
                        var copyOptions = new BlobCopyFromUriOptions
                        {
                            CopySourceTagsMode = BlobCopySourceTagsMode.Replace,
                            Metadata = properties.Metadata,
                        };
                        copyOptions.Metadata["Copy"] = blob.Uri.ToString();

                        newBlob = container.GetBlobClient(request.Body.BlobName);
                        copy = await newBlob.StartCopyFromUriAsync(secureUri, copyOptions, cancellationToken);
                        response = await copy.WaitForCompletionAsync(cancellationToken);
                        if (response.Value == properties.ContentLength)
                        {
                            await blob.DeleteAsync(cancellationToken: cancellationToken);
                            return Unit.Value;
                        }
                        else
                        {
                            throw new Exception(response.GetRawResponse().ReasonPhrase);
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
        }
    }

}
