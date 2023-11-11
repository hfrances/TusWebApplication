using System;
using qckdev;
using System.Security.Cryptography;
using qckdev.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using qckdev.Net.Http;
using System.Configuration;
using TusConsoleApp.Configuration;
using System.Collections.Generic;
using TusClientLibrary;

namespace TusConsoleApp
{
    static class Program
    {

        static void Main(string[] args)
        {
            var commandArgs = CommandArgsDictionary.Create(args);

            if (commandArgs.TryGetValue("store", out string storeName))
            {
                var settings = (TusSettings)ConfigurationManager.GetSection("applicationSettings/tusSettings");
                var containerName = commandArgs.TryGetValue("container", string.Empty);
                var blobName = commandArgs.TryGetValue("blob", string.Empty);
                var replace = commandArgs.TryGetValue("replace", "false").In("", "true");
                var useQueueAsync = commandArgs.TryGetValue("useQueueAsync", "false").In("", "true");

                if (commandArgs.TryGetValue("0", out string fileName))
                {
                    var file = new FileInfo(fileName);

                    if (file.Exists)
                    {
                        Thread.Sleep(2000); // Esperar a que cargue el servidor.

                        /* Upload file */
                        var stw = System.Diagnostics.Stopwatch.StartNew();
                        var client = new TusClient(
                            new Uri(settings.BaseAddress), new TusClientCredentials
                            {
                                UserName = settings.UserName,
                                Login = settings.Login,
                                Password = settings.Password
                            });

                        var uploader = client.CreateFile(
                            file, storeName, containerName,
                            blobName, replace,
                            new Dictionary<string, string>
                            {
                                { "extension", file.Extension }
                            },
                            new Dictionary<string, string>
                            {
                                { "factor", "1,2" }
                            },
                            useQueueAsync
                        );
                        Console.WriteLine($"File:\t\t{uploader}");

                        (int Left, int Top) position = (Console.CursorLeft, Console.CursorTop);
                        uploader.Upload(5D, (transferred, total) =>
                        {
                            Console.SetCursorPosition(position.Left, position.Top);
                            Console.Write($"Progress:\t{(decimal)transferred / total:P2}\t\t{transferred}/{total}");
                        });
                        Console.WriteLine();
                        Console.WriteLine($"Elapsed time:\t{stw.Elapsed}");

                        /* Calculate Hash */
                        string contentHash;
                        using (var md5 = MD5.Create())
                        {
                            using (var stream = File.OpenRead(file.FullName))
                            {
                                contentHash = Convert.ToBase64String(md5.ComputeHash(stream));
                            }
                        }
                        Console.WriteLine($"Hash:\t\t{contentHash}");

                        /* Generate SAS */
                        string sasUrl;
                        sasUrl = client.GenerateSasUrl(uploader.FileUrl, TimeSpan.FromMinutes(10));
                        Console.WriteLine($"Url SAS:\t{sasUrl}");
                    }
                    else
                    {
                        Console.WriteLine("File not found.");
                    }
                }
                else
                {
                    Console.WriteLine("File not specified.");
                }
            }
            else
            {
                Console.WriteLine("Address not specificed.");
            }
            Console.WriteLine();
            Console.ReadKey();
        }

    }
}