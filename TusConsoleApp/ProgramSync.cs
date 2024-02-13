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
    static class ProgramSync
    {

        public static void Run(string[] args)
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
                    var file = new System.IO.FileInfo(fileName);

                    if (file.Exists)
                    {
                        Thread.Sleep(2000); // Esperar a que cargue el servidor.

                        //TestModelo1(settings, storeName, containerName, file, blobName, replace, useQueueAsync);
                        TestModelo3(settings, storeName, containerName, file, blobName, replace, useQueueAsync);

                        Console.WriteLine();
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

        static void TestModelo1(TusSettings settings, string storeName, string containerName, System.IO.FileInfo file, string blobName, bool replace, bool useQueueAsync)
        {

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
                storeName, containerName,
                $"{System.IO.Path.GetFileNameWithoutExtension(file.Name)}+{DateTimeOffset.Now.ToString("s")}{file.Extension}", file.Length,
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
            Console.WriteLine($"File path:\t{uploader.FileUrl}");
            Console.WriteLine($"Relative path:\t{uploader.RelativeUrl}");

            (int Left, int Top) position = (Console.CursorLeft, Console.CursorTop);
            uploader.Upload(file, 5D, (transferred, total) =>
            {
                Console.SetCursorPosition(position.Left, position.Top);
                Console.Write($"Progress:\t{(decimal)transferred / total:P2}\t\t{transferred}/{total}");
            });
            Console.WriteLine();
            Console.WriteLine($"Elapsed time:\t{stw.Elapsed}");

            // Print result.
            PrintResult(client, uploader.FileUrl, file);

        }

        static void TestModelo3(TusSettings settings, string storeName, string containerName, System.IO.FileInfo file, string blobName, bool replace, bool useQueueAsync)
        {
            var stw = System.Diagnostics.Stopwatch.StartNew();
            var client = new TusClient(
                new Uri(settings.BaseAddress), new TusClientCredentials
                {
                    UserName = settings.UserName,
                    Login = settings.Login,
                    Password = settings.Password
                });

            var uploader = client.CreateFile(
                storeName, containerName,
                $"{System.IO.Path.GetFileNameWithoutExtension(file.Name)}+{DateTimeOffset.Now.ToString("s")}{file.Extension}", file.Length,
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
            Console.WriteLine($"File path:\t{uploader.FileUrl}");
            Console.WriteLine($"Relative path:\t{uploader.RelativeUrl}");

            // Upload file in different layer.
            (int Left, int Top) position = (Console.CursorLeft, Console.CursorTop);
            TusClient.UploadFile(uploader.FileUrl, uploader.UploadToken.AccessToken, file, 5D, (transferred, total) =>
            {
                Console.SetCursorPosition(position.Left, position.Top);
                Console.Write($"Progress:\t{(decimal)transferred / total:P2}\t\t{transferred}/{total}");
            });
            Console.WriteLine();
            Console.WriteLine($"Elapsed time:\t{stw.Elapsed}"); ;

            // Print result.
            PrintResult(client, uploader.FileUrl, file);

        }

        static void PrintResult(TusClient client, string fileUrl, FileInfo file)
        {
            /* Calculate Hash */
            string contentHash;
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(file.FullName))
                {
                    contentHash = Convert.ToBase64String(md5.ComputeHash(stream));
                }
            }
            Console.WriteLine($"Hash:\t\t{contentHash}");

            /* Get details */
            FileDetails details;
            details = client.GetFileDetails(fileUrl, includeVersions: true);

            /* Generate SAS */
            string sasUrl;
            sasUrl = client.GenerateSasUrl(fileUrl, TimeSpan.FromMinutes(10));
            Console.WriteLine();
            Console.WriteLine($"Created on:\t{details.CreatedOn}");
            Console.WriteLine($"Url SAS:\t{sasUrl}");

            /* Generate SAS of previous version (if exists) */
            var previousVersion = details.Versions.OrderByDescending(x => x.CreatedOn).FirstOrDefault(x => x.VersionId != details.VersionId);

            if (previousVersion == null)
            {
                Console.WriteLine();
                Console.WriteLine("Previous version: not found.");
            }
            else
            {
                string urlWithVer = $"{fileUrl}?versionId={Uri.EscapeDataString(previousVersion.VersionId)}";
                FileDetails detailsWithVer;
                string sasUrlWithVer;

                detailsWithVer = client.GetFileDetails(urlWithVer);
                sasUrlWithVer = client.GenerateSasUrl(urlWithVer, TimeSpan.FromMinutes(10));
                Console.WriteLine();
                Console.WriteLine("Previous version:");
                Console.WriteLine($"Created on:\t{detailsWithVer.CreatedOn}");
                Console.WriteLine($"Url SAS:\t{sasUrlWithVer}");
            }
        }

    }
}