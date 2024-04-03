namespace TusWebApplication.Application.Auth.Exceptions
{

    sealed class LoginException : qckdev.AspNetCore.HttpHandledException
    {

        public LoginException() :

            base(System.Net.HttpStatusCode.BadRequest, "error.LoginFailed")
        { }

    }

}
