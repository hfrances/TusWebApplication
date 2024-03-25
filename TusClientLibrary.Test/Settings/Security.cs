using System.Configuration;

namespace TusClientLibrary.Test.Settings
{
    public sealed class Security
    {

        public CredentialsConfiguration Credentials { get; set; } = new CredentialsConfiguration();

    }
}
