using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TusClientLibrary.Exceptions
{


    public class TusHandledException : Exception
    {

        internal TusHandledException(string message, Exception innerException)
            : base(message, innerException) 
        { }

    }
}
