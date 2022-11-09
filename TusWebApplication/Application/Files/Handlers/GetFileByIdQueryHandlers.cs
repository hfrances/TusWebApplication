using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TusWebApplication.Application.Files.Dtos;
using TusWebApplication.Application.Files.Queries;
using TusWebApplication.TusAzure;

namespace TusWebApplication.Application.Files.Handlers
{

    sealed class GetFileByIdQueryHandlers : IRequestHandler<GetFileByIdQuery, FileDto>
    {

        Azure.Storage.Blobs.BlobServiceClient BlobService { get; }

        public GetFileByIdQueryHandlers(IOptions<TusAzure.AzureStorageCredentialSettings> azureOptions)
        {
            this.BlobService = TusAzure.TusAzureHelper.CreateBlobServiceClient(
                azureOptions.Value.AccountName ?? string.Empty,
                azureOptions.Value.AccountKey ?? string.Empty
            );
        }

        public async Task<FileDto> Handle(GetFileByIdQuery request, CancellationToken cancellationToken)
        {
            var container = BlobService.GetBlobContainerClient(request.ContainerName);
            var blob = container.GetBlobClient(request.BlobName);
            var properties = (await blob.GetPropertiesAsync()).Value;
            var tags = (await blob.GetTagsAsync(cancellationToken: cancellationToken)).Value.Tags;
            Uri uri;

            if (request.Parameters.GenerateSas && blob.CanGenerateSasUri)
            {
                uri = blob.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(12));
            }
            else
            {
                uri = blob.Uri;
            }
            return new FileDto
            {
                BlobId = $"{container.Name}/{blob.Name}",
                Name = tags.SingleOrDefault(x => x.Key.Equals("filename", System.StringComparison.OrdinalIgnoreCase)).Value,
                Length = properties.ContentLength,
                Metadata = properties.Metadata,
                Tags = tags.Where(x => !x.Key.Equals("filename", System.StringComparison.OrdinalIgnoreCase)).ToDictionary(x => x.Key, x => x.Value),
                Url = uri,
                Checksum = (properties.ContentHash == null) ? 
                    null : 
                    Convert.ToBase64String(properties.ContentHash),
                CreatedOn = properties.CreatedOn,
            };
        }
    }

}
