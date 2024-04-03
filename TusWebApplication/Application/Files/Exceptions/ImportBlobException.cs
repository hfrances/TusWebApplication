namespace TusWebApplication.Application.Files.Exceptions
{

    sealed class ImportBlobException : qckdev.AspNetCore.HttpHandledException
    {

        public ImportBlobException(System.Exception innerException) :
            this(System.Net.HttpStatusCode.InternalServerError, innerException)
        { }

        public ImportBlobException(qckdev.AspNetCore.HttpHandledException innerException) :
            this(innerException.ErrorCode, innerException)
        { }

        public ImportBlobException(System.Net.HttpStatusCode errorCode, System.Exception innerException) :
            base(errorCode, "error.ImportBlobException", innerException)
        { }

    }

}
