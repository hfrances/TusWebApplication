using System;
using qckdev;
using System.Security.Cryptography;
using qckdev.Linq;
using System.Threading;
using System.Configuration;
using TusConsoleApp.Configuration;
using System.Collections.Generic;
using TusClientLibrary;
using System.Linq;
using System.IO;

namespace TusConsoleApp
{
    static class Program
    {

        static void Main(string[] args)
            => ProgramSync.Run(args);

#if NO_ASYNC
#else
        static System.Threading.Tasks.Task Main2(string[] args)
        => ProgramAsync.Run(args);

#endif
    }
}