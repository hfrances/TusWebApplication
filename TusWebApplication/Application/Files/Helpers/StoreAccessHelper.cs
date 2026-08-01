using TusWebApplication.AzureBlobProvider;

namespace TusWebApplication.Application.Files.Helpers
{

    static class StoreAccessHelper
    {

        public static void EnsureWritable(AzureStorageCredentialSettings settings)
        {
            if (settings.ReadOnly)
            {
                throw new Exceptions.ReadOnlyStoreException();
            }
        }

    }

}
