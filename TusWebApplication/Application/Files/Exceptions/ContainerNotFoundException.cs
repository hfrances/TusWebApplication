namespace TusWebApplication.Application.Files.Exceptions
{

    sealed class ContainerNotFoundException : qckdev.AspNetCore.HttpHandledException
    {

        public ContainerNotFoundException() : 
            base(System.Net.HttpStatusCode.NotFound, "error.ContainerNotFound")
        { }

    }

}
