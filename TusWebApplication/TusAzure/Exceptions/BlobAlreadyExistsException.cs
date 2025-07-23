namespace TusWebApplication.TusAzure.Exceptions
{

    sealed class BlobAlreadyExistsException : qckdev.AspNetCore.HttpHandledException
    {

        // TODO: Poner este mensaje.
        // $"Blob {storeName}/{blobId} already exists. Set 'replace' argument to overwrite it."
        // Tener un id de error.
        public BlobAlreadyExistsException(string storeName, string blobId) :
            base(System.Net.HttpStatusCode.BadRequest, "error.BlobAlreadyExists")
        { }

    }

}
