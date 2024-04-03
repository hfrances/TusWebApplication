using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace TusWebApplication.TusAzure.Authentication
{
    static class DependencyInjection
    {

        public static AuthenticationBuilder AddTusJwtBearer(this AuthenticationBuilder builder, JwtTokenConfiguration configuration)
        {

            return builder
                .AddJwtBearer(Constants.UPLOAD_FILE_SCHEMA, options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(configuration.Key)),
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateLifetime = true,
                        ClockSkew = System.TimeSpan.Zero
                    };
                },
                (JwtBearerMoreOptions moreOptions) =>
                {
                    moreOptions.TokenLifeTimespan = System.TimeSpan.FromSeconds(configuration.AccessExpireSeconds);
                    moreOptions.FirstRequestLifeTimeSpan = System.TimeSpan.FromSeconds(configuration.FirstRequestExpireSeconds);
                });
        }

    }
}
