namespace TusWebApplication.Application.Files.Exceptions
{

    sealed class BlobStorageNotFoundException : qckdev.AspNetCore.HttpHandledException
    {

        public BlobStorageNotFoundException() :
            base(System.Net.HttpStatusCode.NotFound, "error.BlobStorageNotFound")
        { }

    }

}
