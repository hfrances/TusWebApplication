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
    static class ProgramAsync
    {

        public static async Task Run(string[] args)
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

                        //await TestModelo1Async(settings, storeName, containerName, file, blobName, replace, useQueueAsync);
                        //await TestModelo3Async(settings, storeName, containerName, file, blobName, replace, useQueueAsync);
                        await TestModelo3Async_ByStream(settings, storeName, containerName, file, blobName, replace, useQueueAsync);

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

        static async Task TestModelo1Async(TusSettings settings, string storeName, string containerName, System.IO.FileInfo file, string blobName, bool replace, bool useQueueAsync)
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

            var uploader = await client.CreateFileAsync(
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
                useQueueAsync,
                Common.CalculateMD5(file.FullName)
            );
            Console.WriteLine($"File path:\t{uploader.FileUrl}");
            Console.WriteLine($"Relative path:\t{uploader.RelativeUrl}");

            (int Left, int Top) position = (Console.CursorLeft, Console.CursorTop);
            await uploader.UploadAsync(file, 5D, (transferred, total) =>
            {
                Console.SetCursorPosition(position.Left, position.Top);
                Console.Write($"Progress:\t{(decimal)transferred / total:P2}\t\t{transferred}/{total}");
            });
            Console.WriteLine();
            Console.WriteLine($"Elapsed time:\t{stw.Elapsed}");

            // Print result.
            await PrintResultAsync(client, uploader.FileUrl, file);

        }

        static async Task TestModelo3Async(TusSettings settings, string storeName, string containerName, System.IO.FileInfo file, string blobName, bool replace, bool useQueueAsync)
        {
            var stw = System.Diagnostics.Stopwatch.StartNew();
            var client = new TusClient(
                new Uri(settings.BaseAddress), new TusClientCredentials
                {
                    UserName = settings.UserName,
                    Login = settings.Login,
                    Password = settings.Password
                });

            var uploader = await client.CreateFileAsync(
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
                useQueueAsync,
                Common.CalculateMD5(file.FullName)
            );
            Console.WriteLine($"File path:\t{uploader.FileUrl}");
            Console.WriteLine($"Relative path:\t{uploader.RelativeUrl}");

            // Upload file in different layer.
            (int Left, int Top) position = (Console.CursorLeft, Console.CursorTop);
            await TusClient.UploadFileAsync(uploader.FileUrl, uploader.UploadToken.AccessToken, file, 5D, (transferred, total) =>
            {
                Console.SetCursorPosition(position.Left, position.Top);
                Console.Write($"Progress:\t{(decimal)transferred / total:P2}\t\t{transferred}/{total}");
            });
            Console.WriteLine();
            Console.WriteLine($"Elapsed time:\t{stw.Elapsed}"); ;

            // Print result.
            await PrintResultAsync(client, uploader.FileUrl, file);

        }

        static async Task TestModelo3Async_ByStream(TusSettings settings, string storeName, string containerName, System.IO.FileInfo file, string blobName, bool replace, bool useQueueAsync)
        {
            var stw = System.Diagnostics.Stopwatch.StartNew();
            var client = new TusClient(
                new Uri(settings.BaseAddress), new TusClientCredentials
                {
                    UserName = settings.UserName,
                    Login = settings.Login,
                    Password = settings.Password
                });

            var uploader = await client.CreateFileAsync(
                storeName, containerName,
                $"{System.IO.Path.GetFileNameWithoutExtension(file.Name)}+{DateTimeOffset.Now.ToString("s")}{file.Extension}", file.Length,
                blobName, replace,
                new CreateFileOptions
                {
                    ContentType = Common.CalculateMimeType(file.FullName),
                    Tags = new Dictionary<string, string>
                    {
                        { "extension", file.Extension }
                    },
                    Metadata = new Dictionary<string, string>
                    {
                        { "factor", "1,2" }
                    },
                    UseQueueAsync = useQueueAsync,
                    Hash = Common.CalculateMD5(file.FullName)
                }
            );
            Console.WriteLine($"File path:\t{uploader.FileUrl}");
            Console.WriteLine($"Relative path:\t{uploader.RelativeUrl}");

            // Upload file in different layer.
            using (var stream = file.OpenRead())
            {
                (int Left, int Top) position = (Console.CursorLeft, Console.CursorTop);
                await TusClient.UploadFileAsync(uploader.FileUrl, uploader.UploadToken.AccessToken, stream, 5D, (transferred, total) =>
                {
                    Console.SetCursorPosition(position.Left, position.Top);
                    Console.Write($"Progress:\t{(decimal)transferred / total:P2}\t\t{transferred}/{total}");
                });
                Console.WriteLine();
                Console.WriteLine($"Elapsed time:\t{stw.Elapsed}"); ;
            }

            // Print result.
            await PrintResultAsync(client, uploader.FileUrl, file);

        }


        static async Task PrintResultAsync(TusClient client, string fileUrl, FileInfo file)
        {
            /* Calculate Hash */
            string contentHash;
            contentHash = Common.CalculateMD5(file.FullName);
            Console.WriteLine($"Hash:\t\t{contentHash}");
            Console.WriteLine();

            /* Get details */
            FileDetails details = null;
            Console.WriteLine("Checking file status...");
            do
            {
                if (details != null)
                {
                    System.Threading.Thread.Sleep(1000);
                }
                details = await client.GetFileDetailsAsync(fileUrl, includeVersions: true);
                Console.WriteLine($"Status: {details.Status?.ToString() ?? "(null)"}; Percentaje: {details.UploadPercentage?.ToString("P") ?? "(null)"}");
            }
            while (details.Status == UploadStatus.Uploading);
            Console.WriteLine();

            /* Generate SAS */
            Uri sasUri, sasUriInline, sasUriAttachment;
            sasUri = await client.GenerateSasUrlAsync(new Uri(fileUrl), TimeSpan.FromMinutes(10));
            Console.WriteLine($"Created on:\t{details.CreatedOn}");
            Console.WriteLine($"Url SAS:\t{sasUri}");
            sasUriInline = sasUri.WithQueryValues(new Dictionary<string, string>() { { "inline", "true" } });
            Console.WriteLine($"        \t{sasUriInline}");
            sasUriAttachment = sasUri.WithQueryValues(new Dictionary<string, string>() { { "inline", "false" } });
            Console.WriteLine($"        \t{sasUriAttachment}");

            /* Generate SAS of previous version (if exists) */
            var previousVersion = details.Versions?.OrderByDescending(x => x.CreatedOn).FirstOrDefault(x => x.VersionId != details.VersionId);

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

                detailsWithVer = await client.GetFileDetailsAsync(urlWithVer);
                sasUrlWithVer = await client.GenerateSasUrlAsync(urlWithVer, TimeSpan.FromMinutes(10));
                Console.WriteLine();
                Console.WriteLine("Previous version:");
                Console.WriteLine($"Created on:\t{detailsWithVer.CreatedOn}");
                Console.WriteLine($"Url SAS:\t{sasUrlWithVer}");
            }
        }

    }
}