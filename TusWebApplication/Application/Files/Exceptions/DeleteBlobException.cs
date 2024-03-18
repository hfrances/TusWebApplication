namespace TusWebApplication.Application.Files.Exceptions
{

    sealed class DeleteBlobException : qckdev.AspNetCore.HttpHandledException
    {

        public DeleteBlobException(System.Exception innerException) :
            this(System.Net.HttpStatusCode.InternalServerError, innerException)
        { }

        public DeleteBlobException(qckdev.AspNetCore.HttpHandledException innerException) :
            this(innerException.ErrorCode, innerException)
        { }

        public DeleteBlobException(System.Net.HttpStatusCode errorCode, System.Exception innerException) :
            base(errorCode, "error.DeleteBlobException", innerException)
        { }

    }

}
