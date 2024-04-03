using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;
using TusWebApplication.Application.Files.Commands;
using TusWebApplication.Application.Files.Dtos;

namespace TusWebApplication.Application.Files.Handlers
{
    sealed class RequestUploadCommandHandler : IRequestHandler<RequestUploadCommand, RequestUploadDto>
    {

        IOptionsMonitor<JwtBearerOptions> JwtBearerOptions { get; }
        IOptionsMonitor<TusAzure.Authentication.JwtBearerMoreOptions> JwtBearerMoreOptions { get; }
        AzureBlobProvider.AzureStorageCredentialsSettings AzureSettings { get; }

        public RequestUploadCommandHandler(
            IOptionsMonitor<JwtBearerOptions> jwtBearerOptions,
            IOptionsMonitor<TusAzure.Authentication.JwtBearerMoreOptions> jwtBearerMoreOptions,
            IOptionsMonitor<AzureBlobProvider.AzureStorageCredentialsSettings> azureOptions)
        {
            this.JwtBearerOptions = jwtBearerOptions;
            this.JwtBearerMoreOptions = jwtBearerMoreOptions;
            this.AzureSettings = azureOptions.CurrentValue;
        }


        public async Task<RequestUploadDto> Handle(RequestUploadCommand request, CancellationToken cancellationToken)
        {
            var issuerSigningKey = JwtBearerOptions.Get(TusAzure.Authentication.Constants.UPLOAD_FILE_SCHEMA)?.TokenValidationParameters.IssuerSigningKey;
            var tokenLifeTimespan = JwtBearerMoreOptions.Get(TusAzure.Authentication.Constants.UPLOAD_FILE_SCHEMA)?.TokenLifeTimespan;
            var firstRequestLifeTimespan = JwtBearerMoreOptions.Get(TusAzure.Authentication.Constants.UPLOAD_FILE_SCHEMA)?.FirstRequestLifeTimeSpan;

            if (firstRequestLifeTimespan == null)
            {
                throw new NullReferenceException("First request life parameter cannot be found."); // TODO: Crear excepción propia.
            }
            else if (AzureSettings.TryGetValue(request.StoreName, out AzureBlobProvider.AzureStorageCredentialSettings? azureSettings))
            {
                var blobService = AzureBlobProvider.AzureBlobHelper.CreateBlobServiceClient(
                        azureSettings.AccountName, azureSettings.AccountKey
                    );
                var container = blobService.GetBlobContainerClient(request.Container);

                if (await container.ExistsAsync(cancellationToken))
                {
                    qckdev.Authentication.JwtBearer.JwtToken token;
                    var blobName =
                        string.IsNullOrWhiteSpace(request.Body.Blob) ? // If blobName was not set, create a new automatically.
                            Guid.NewGuid().ToString()
                            : request.Body.Blob;
                    var blobId = $"{container.Name}/{blobName}";
                    var firstRequestExpired = DateTimeOffset.UtcNow.Add(firstRequestLifeTimespan.Value);
                    var properties = new TusAzure.Authentication.UploadProperties
                    {
                        Container = request.Container,
                        FileName = request.Body.FileName,
                        Blob = blobName,
                        BlobId = blobId,
                        ContentType = request.Body.ContentType,
                        ContentLanguage = request.Body.ContentLanguage,
                        Replace = request.Body.Replace,
                        Size = request.Body.Size,
                        Hash = request.Body.Hash,
                        UseQueueAsync = request.Body.UseQueueAsync,
                        FirstRequestExpired = firstRequestExpired
                    };

                    token = qckdev.Authentication.JwtBearer.JwtGenerator.CreateToken(
                        issuerSigningKey, "blob",
                        claims: TusAzure.Authentication.TusAuthenticationHelper.CreateClaims(properties),
                        lifespan: tokenLifeTimespan
                    );

                    return new RequestUploadDto
                    {
                        StoreName = request.StoreName,
                        BlobId = blobId,
                        AccessToken = token.AccessToken,
                        Expired = firstRequestExpired
                    };
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
    }
}
