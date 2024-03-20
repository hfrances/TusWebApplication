using System.Configuration;

namespace TusClientLibrary.Test.Settings
{
    public sealed class Configuration
    {

        public Security Security { get; set; } = new Security();

    }
}
