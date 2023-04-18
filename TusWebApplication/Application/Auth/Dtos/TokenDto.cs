using System;

namespace TusWebApplication.Application.Auth.Dtos
{

    public sealed class TokenDto
    {

        public string UserName { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public DateTimeOffset Expired { get; set; }

    }

}
