using System;

namespace TusWebApplication.AzureBlobProvider
{
    static class AzureBlobHelper
    {

        public static Azure.Storage.Blobs.BlobServiceClient CreateBlobServiceClient(string accountName, string accountKey)
        {
            var credentials = new Azure.Storage.StorageSharedKeyCredential(accountName, accountKey);
            var blobUri = new Uri($"https://{accountName}.blob.core.windows.net");

            return new Azure.Storage.Blobs.BlobServiceClient(blobUri, credentials);
        }

    }
}
