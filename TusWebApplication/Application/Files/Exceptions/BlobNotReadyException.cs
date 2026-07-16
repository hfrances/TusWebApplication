namespace TusWebApplication.Application.Files.Exceptions
{

    sealed class BlobNotReadyException : qckdev.AspNetCore.HttpHandledException
    {

        public BlobNotReadyException() :
            base(System.Net.HttpStatusCode.NotFound, "error.BlobNotReady")
        { }

    }

}
