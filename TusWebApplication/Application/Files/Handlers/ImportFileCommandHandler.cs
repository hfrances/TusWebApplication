using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TusWebApplication.Application.Files.Commands;
using TusWebApplication.Application.Files.Dtos;

namespace TusWebApplication.Application.Files.Handlers
{

    sealed class ImportFileCommandHandler : IRequestHandler<ImportFileCommand, ImportDto>
    {

        AzureBlobProvider.AzureStorageCredentialsSettings AzureSettings { get; }

        public ImportFileCommandHandler(IOptions<AzureBlobProvider.AzureStorageCredentialsSettings> azureOptions)
        {
            this.AzureSettings = azureOptions.Value;
        }

        public async Task<ImportDto> Handle(ImportFileCommand request, CancellationToken cancellationToken)
        {

            try
            {
                if (AzureSettings.TryGetValue(request.StoreName, out AzureBlobProvider.AzureStorageCredentialSettings? settings))
                {
                    var blobService = AzureBlobProvider.AzureBlobHelper.CreateBlobServiceClient(
                        settings.AccountName, settings.AccountKey
                    );
                    var container = blobService.GetBlobContainerClient(request.ContainerName);

                    if (request.Body == null)
                    {
                        throw new NullReferenceException("Body not found.");
                    }
                    else if (string.IsNullOrWhiteSpace(request.Body.SourceUrl))
                    {
                        throw new NullReferenceException("Source Url not specified.");
                    }
                    else
                    {
                        string blobName = request.Body.TargetBlobName ?? Guid.NewGuid().ToString();

                        if (await container.ExistsAsync(cancellationToken))
                        {
                            BlobClient blob, newBlob;

                            blob = container.GetBlobClient(blobName);
                            if (await blob.ExistsAsync(cancellationToken))
                            {
                                throw new Exceptions.BlobAlreadyExistsException();
                            }
                            else
                            {
                                CopyFromUriOperation copy;
                                Azure.Response responseRaw;
                                var copyOptions = new BlobCopyFromUriOptions
                                {
                                    CopySourceTagsMode = BlobCopySourceTagsMode.Copy,
                                    Tags = null, // Not set here because then it not imports orginal tags.
                                    Metadata = null // Not set here because then it not imports original metadatas,
                                };

                                newBlob = container.GetBlobClient(request.Body.TargetBlobName);
                                copy = await newBlob.StartCopyFromUriAsync(new Uri(request.Body.SourceUrl), copyOptions, cancellationToken);
                                responseRaw = copy.GetRawResponse();
                                if (responseRaw.IsError)
                                {
                                    throw new Exception(responseRaw.ReasonPhrase);
                                }
                                else
                                {
                                    BlobProperties properties = await blob.GetPropertiesAsync();

                                    var task =
                                        copy.WaitForCompletionAsync(cancellationToken)
                                            .AsTask()
                                            .ContinueWith(async x =>
                                            {
                                                Azure.Response<long> response;

                                                response = x.Result;
                                                responseRaw = response.GetRawResponse();

                                                if (responseRaw.IsError)
                                                {
                                                    throw new Exception(responseRaw.ReasonPhrase);
                                                }
                                                else
                                                {
                                                    Azure.Response<BlobInfo> responseBlob;
                                                    bool metadataModified, tagsModified;
                                                    var currentMetadata = properties.Metadata;
                                                    var finalMetadata = MergeMetadata(currentMetadata, request.Body.FileName, request.Body.Metadata, out metadataModified);
                                                    var currentTags = (await blob.GetTagsAsync()).Value.Tags;
                                                    var finalTags = MergeTags(currentTags, request.Body.Tags, out tagsModified);

                                                    if (!string.IsNullOrWhiteSpace(request.Body.ContentType))
                                                    {
                                                        responseBlob = await newBlob.SetHttpHeadersAsync(new BlobHttpHeaders
                                                        {
                                                            ContentType = request.Body.ContentType,
                                                        });
                                                        responseRaw = responseBlob.GetRawResponse();
                                                        if (responseRaw.IsError)
                                                        {
                                                            throw new Exception(responseRaw.ReasonPhrase);
                                                        }
                                                    }
                                                    if (metadataModified)
                                                    {
                                                        responseBlob = await blob.SetMetadataAsync(finalMetadata);
                                                        responseRaw = responseBlob.GetRawResponse();
                                                        if (responseRaw.IsError)
                                                        {
                                                            throw new Exception(responseRaw.ReasonPhrase);
                                                        }
                                                    }
                                                    if (tagsModified)
                                                    {
                                                        responseRaw = await blob.SetTagsAsync(finalTags);
                                                        if (responseRaw.IsError)
                                                        {
                                                            throw new Exception(responseRaw.ReasonPhrase);
                                                        }
                                                    }
                                                }
                                            });
                                    if (request.Body.WaitForComplete)
                                    {
                                        task.Wait();
                                    }
                                    return new ImportDto
                                    {
                                        StoreName = request.StoreName,
                                        BlobId = $"{container.Name}/{blob.Name}",
                                        VersionId = properties.VersionId
                                    };
                                }
                            }
                        }
                        else
                        {
                            throw new Exceptions.ContainerNotFoundException();
                        }
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

        static IDictionary<string, string> MergeMetadata(IDictionary<string, string> original, string? fileName, IDictionary<string, string>? append, out bool modified)
        {
            IDictionary<string, string> result = original ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            modified = false;
            if (append != null)
            {
                foreach (var item in append)
                {
                    result[item.Key] = Uri.EscapeDataString(item.Value);
                    modified = true;
                }
            }
            if (!string.IsNullOrWhiteSpace(fileName))
            {
                result["filename"] = Uri.EscapeDataString(fileName);
                modified = true;
            }
            return result;
        }

        static IDictionary<string, string> MergeTags(IDictionary<string, string> original, IDictionary<string, string>? append, out bool modified)
        {
            IDictionary<string, string> result = original ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            modified = false;
            if (append != null)
            {
                foreach (var item in append)
                {
                    result[item.Key] = item.Value;
                    modified = true;
                }
            }
            return result;
        }

    }

}
