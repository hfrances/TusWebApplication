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
            
            if (firstRequestLifeTimespan == null)
            {
                throw new NullReferenceException("First request life cannot be found."); // TODO: Crear excepción propia.
            }
            else
            {
                qckdev.Authentication.JwtBearer.JwtToken token;
                var firstRequestExpired = DateTimeOffset.UtcNow.Add(firstRequestLifeTimespan.Value);
                var properties = new TusAzure.Authentication.UploadProperties
                {
                    Container = request.Container,
                    FileName = request.Body.FileName,
                    Blob = request.Body.Blob,
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

                return Task.FromResult(new RequestUploadDto
                {
                    AccessToken = token.AccessToken,
                    Expired = firstRequestExpired
                });
            }
        }
    }
}
