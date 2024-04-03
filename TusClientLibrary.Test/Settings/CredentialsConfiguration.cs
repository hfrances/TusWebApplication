using System.Configuration;

namespace TusClientLibrary.Test.Settings
{
    public sealed class CredentialsConfiguration
    {

        public string UserName { get; } = "test";
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

    }
}
