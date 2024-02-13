using System;
using qckdev;
using System.Security.Cryptography;
using qckdev.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using qckdev.Net.Http;
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

        static void Main2(string[] args)
            => ProgramSync.Run(args);

        static async Task Main(string[] args)
            => ProgramSync.Run(args);

    }
}