namespace TusWebApplication.TusAzure
{
    interface IBlobManager
    {

        BlobStatus? GetBlobStatus(string storeName, string container, string blobName);

    }
}
