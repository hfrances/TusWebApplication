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
        IHttpContextAccessor HttpContextAccessor { get; }

        public GenerateSasTokenFromFileIdCommandHandler(
            IOptions<AzureBlobProvider.AzureStorageCredentialsSettings> azureOptions,
            TusAzure.IBlobManager tusAzureBlobManager,
            IHttpContextAccessor httpContextAccessor)
        {
            this.AzureSettings = azureOptions.Value;
            this.TusAzureBlobManager = tusAzureBlobManager;
            this.HttpContextAccessor = httpContextAccessor;
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
                            var builder = new UriBuilder($"{HttpContextAccessor.HttpContext.Request.Scheme}://{HttpContextAccessor.HttpContext.Request.Host.Value}")
                            {
                                Path = string.Join("/", HttpContextAccessor.HttpContext.Request.Path.Value.Split('/').SkipLast(1))
                            };
                            
                            var query = new Dictionary<string, string>();
                            if (request.Parameters?.VersionId != null)
                            {
                                query.Add("versionId", request.Parameters.VersionId);
                            }
                            query.Add("sv", "1");
                            query.Add("se", request.Body.ExpiresOn.ToString("s"));
                            query.Add("sig", token);
                            builder.Query = QueryHelpers.AddQueryString("", query);
                            return builder.Uri.AbsoluteUri;

                        }, cancellationToken),
                cancellationToken);

    }
}
