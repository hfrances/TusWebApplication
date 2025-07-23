using System;

namespace TusClientLibrary.Exceptions
{

    public sealed class LoginException : TusHandledException
    {

        internal LoginException(string message, Exception innerException) :
            base(message, innerException)
        { }

    }

}
