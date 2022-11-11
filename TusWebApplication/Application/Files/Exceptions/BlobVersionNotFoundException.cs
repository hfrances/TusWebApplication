using System;
using System.Runtime.Serialization;

namespace TusWebApplication.Application.Files.Exceptions
{

    sealed class BlobVersionNotFoundException : qckdev.AspNetCore.HttpHandledException
    {

        public BlobVersionNotFoundException() :
            base(System.Net.HttpStatusCode.NotFound, "error.BlobVersionNotFound")
        { }

    }
}