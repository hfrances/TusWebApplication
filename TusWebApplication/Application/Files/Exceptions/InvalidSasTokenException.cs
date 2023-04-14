namespace TusWebApplication.Application.Files.Exceptions
{
    sealed class InvalidSasTokenException: qckdev.AspNetCore.HttpHandledException
    {

        const string ERROR_MESSAGE = "Server failed to authenticate the request. Make sure the value of Authorization header is formed correctly including the signature.";

        public InvalidSasTokenException(string errorDetails)
            :base(System.Net.HttpStatusCode.Forbidden, ERROR_MESSAGE)
        { 
            this.Content = errorDetails;
        }

    }
}
