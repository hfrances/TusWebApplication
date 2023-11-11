using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using qckdev.AspNetCore.Authentication.JwtBearer;
using System;
using System.Collections.Generic;
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


        public RequestUploadCommandHandler(
            IOptionsMonitor<JwtBearerOptions> jwtBearerOptions, IOptionsMonitor<TusAzure.Authentication.JwtBearerMoreOptions> jwtBearerMoreOptions,
            IOptionsMonitor<Settings.CredentialsConfiguration> credentialsOptions)
        {
            this.JwtBearerOptions = jwtBearerOptions;
            this.JwtBearerMoreOptions = jwtBearerMoreOptions;
        }

        public Task<RequestUploadDto> Handle(RequestUploadCommand request, CancellationToken cancellationToken)
        {
            var issuerSigningKey = JwtBearerOptions.Get(TusAzure.Authentication.Constants.UPLOAD_FILE_SCHEMA)?.TokenValidationParameters.IssuerSigningKey;
            var tokenLifeTimespan = JwtBearerMoreOptions.Get(TusAzure.Authentication.Constants.UPLOAD_FILE_SCHEMA)?.TokenLifeTimespan;
            var firstRequestLifeTimespan = JwtBearerMoreOptions.Get(TusAzure.Authentication.Constants.UPLOAD_FILE_SCHEMA)?.FirstRequestLifeTimeSpan;
            var claims = new List<System.Security.Claims.Claim>();
            qckdev.Authentication.JwtBearer.JwtToken token;

            claims.Add(new System.Security.Claims.Claim("container", request.Container));
            claims.Add(new System.Security.Claims.Claim("file-name", request.Body.FileName));
            claims.Add(new System.Security.Claims.Claim("blob", request.Body.Blob ?? ""));
            claims.Add(new System.Security.Claims.Claim("replace", request.Body.Replace.ToString()));
            claims.Add(new System.Security.Claims.Claim("size", request.Body.Size.ToString(System.Globalization.CultureInfo.InvariantCulture)));
            claims.Add(new System.Security.Claims.Claim("hash", request.Body.Hash ?? ""));

            if (firstRequestLifeTimespan == null)
            {
                throw new NullReferenceException("First request life cannot be found."); // TODO: Crear excepción propia.
            }
            else
            {
                var firstRequestExpired = DateTimeOffset.UtcNow.Add(firstRequestLifeTimespan.Value);

                claims.Add(new System.Security.Claims.Claim("expired", firstRequestExpired.ToString("O", System.Globalization.CultureInfo.InvariantCulture)));

                token = qckdev.Authentication.JwtBearer.JwtGenerator.CreateToken(
                    issuerSigningKey, "blob",
                    claims: claims,
                    lifespan: tokenLifeTimespan
                );

                return Task.FromResult(new RequestUploadDto
                {
                    AccessToken = token.AccessToken,
                    Expired = firstRequestExpired
                });
            }
        }
    }
}
