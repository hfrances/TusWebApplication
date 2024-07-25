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

#if NO_ASYNC
        static void Main(string[] args)
            => ProgramSync.Run(args);
#else        
        static void Main2(string[] args)
            => ProgramSync.Run(args);

        static System.Threading.Tasks.Task Main(string[] args)
            => ProgramAsync.Run(args);
#endif

    }
}