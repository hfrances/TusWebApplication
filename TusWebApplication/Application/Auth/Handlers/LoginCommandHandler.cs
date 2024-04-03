using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using qckdev.AspNetCore.Authentication.JwtBearer;
using System;
using System.Threading;
using System.Threading.Tasks;
using TusWebApplication.Application.Auth.Commands;
using TusWebApplication.Application.Auth.Dtos;

namespace TusWebApplication.Application.Auth.Handlers
{
    sealed class LoginCommandHandler : IRequestHandler<LoginCommand, TokenDto>
    {

        IOptionsMonitor<JwtBearerOptions> JwtBearerOptions { get; }
        IOptionsMonitor<JwtBearerMoreOptions> JwtBearerMoreOptions { get; }
        IOptionsMonitor<Settings.CredentialsConfiguration> CredentialsOptions { get; }

        public LoginCommandHandler(
            IOptionsMonitor<JwtBearerOptions> jwtBearerOptions, IOptionsMonitor<JwtBearerMoreOptions> jwtBearerMoreOptions,
            IOptionsMonitor<Settings.CredentialsConfiguration> credentialsOptions)
        {
            this.JwtBearerOptions = jwtBearerOptions;
            this.JwtBearerMoreOptions = jwtBearerMoreOptions;
            this.CredentialsOptions = credentialsOptions;
        }

        public Task<TokenDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var issuerSigningKey = JwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme)?.TokenValidationParameters.IssuerSigningKey;
            var tokenLifeTimespan = JwtBearerMoreOptions.Get(JwtBearerDefaults.AuthenticationScheme)?.TokenLifeTimespan;
            var credentialsSettings = CredentialsOptions.CurrentValue;

            if (issuerSigningKey == null)
            {
                throw new Exceptions.LoginException();
            }
            else if (string.IsNullOrWhiteSpace(request.UserName))
            {
                throw new Exceptions.LoginException();
            }
            else if (!request.Login.Equals(credentialsSettings.Login, StringComparison.CurrentCulture) || !request.Password.Equals(credentialsSettings.Password))
            {
                throw new Exceptions.LoginException();
            }
            else
            {
                var token = qckdev.Authentication.JwtBearer.JwtGenerator.CreateToken(
                    issuerSigningKey, request.UserName,
                    lifespan: tokenLifeTimespan
                );

                return Task.FromResult(new TokenDto
                {
                    UserName = request.UserName,
                    AccessToken = token.AccessToken,
                    Expired = token.Expired
                });
            }
        }

    }
}
