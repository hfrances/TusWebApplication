
using System;
using System.Collections.Generic;

namespace TusWebApplication.AzureBlobProvider
{
    public sealed class AzureStorageCredentialsSettings
        : Dictionary<string, AzureStorageCredentialSettings>
    {

        public AzureStorageCredentialsSettings() : base(StringComparer.OrdinalIgnoreCase)
        { }

    }
}
