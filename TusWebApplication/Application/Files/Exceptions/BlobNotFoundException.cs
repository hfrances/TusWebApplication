namespace TusWebApplication.Application.Files.Exceptions
{

    sealed class BlobNotFoundException : qckdev.AspNetCore.HttpHandledException
    {

        public BlobNotFoundException() : 
            base(System.Net.HttpStatusCode.NotFound, "error.BlobNotFound")
        { }

    }

}
