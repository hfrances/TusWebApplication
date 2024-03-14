namespace TusWebApplication.Application.Files.Exceptions
{

    sealed class BlobAlreadyExistsException : qckdev.AspNetCore.HttpHandledException
    {

        public BlobAlreadyExistsException() : 
            base(System.Net.HttpStatusCode.BadRequest, "error.BlobAlreadyExists")
        { }

    }

}
