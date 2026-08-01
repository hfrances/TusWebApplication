namespace TusWebApplication.Application.Files.Exceptions
{

    sealed class ReadOnlyStoreException : qckdev.AspNetCore.HttpHandledException
    {

        public ReadOnlyStoreException() :
            base(System.Net.HttpStatusCode.Forbidden, "error.ReadOnlyStore")
        { }

    }

}
