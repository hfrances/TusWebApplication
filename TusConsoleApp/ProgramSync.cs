using System;
using qckdev;
using qckdev.Linq;
using System.Threading;
using System.Configuration;
using TusConsoleApp.Configuration;
using System.Collections.Generic;
using TusClientLibrary;
using System.Linq;
using System.IO;
using System.Drawing;

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
                var sasMinutes = commandArgs.TryGetValue("sas", "10");
                var deleteOnFinish = commandArgs.TryGetValue("delete", "false").In("", "true");

                if (commandArgs.TryGetValue("0", out string fileName))
                {
                    var file = new System.IO.FileInfo(fileName);
                    int sasMinutesInt;

                    if (!int.TryParse(sasMinutes, out sasMinutesInt))
                    {
                        Console.WriteLine("Invalid format for parameter 'sas'. It must be a numeric value.");
                    }
                    else if (file.Exists)
                    {
                        Thread.Sleep(2000); // Esperar a que cargue el servidor.

                        //TestModelo1(settings, storeName, containerName, file, blobName, replace, useQueueAsync, sasMinutesInt, deleteOnFinish);
                        //TestModelo3(settings, storeName, containerName, file, blobName, replace, useQueueAsync, sasMinutesInt, deleteOnFinish);
                        TestModelo3_ByStream(settings, storeName, containerName, file, blobName, replace, useQueueAsync, sasMinutesInt, deleteOnFinish);

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

        static void TestModelo1(TusSettings settings, string storeName, string containerName, System.IO.FileInfo file, string blobName, bool replace, bool useQueueAsync, int sasMinutes = 10, bool deleteOnFinish = false)
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
                useQueueAsync,
                Common.CalculateMD5(file.FullName)
            );
            Console.WriteLine($"File path:\t{uploader.FileUrl}");
            Console.WriteLine($"Relative path:\t{uploader.RelativeUrl}");


            var position = new Point(Console.CursorLeft, Console.CursorTop);
            uploader.Upload(file, 5D, (transferred, total) =>
            {
                Console.SetCursorPosition(position.X, position.Y);
                Console.Write($"Progress:\t{(decimal)transferred / total:P2}\t\t{transferred}/{total}");
            });
            Console.WriteLine();
            Console.WriteLine($"Elapsed time:\t{stw.Elapsed}");

            // Print result.
            PrintResult(client, uploader.FileUrl, file, sasMinutes);

            // Delete blob.
            if (deleteOnFinish)
            {
                Delete(client, uploader.FileUrl);
            }
        }

        static void TestModelo3(TusSettings settings, string storeName, string containerName, System.IO.FileInfo file, string blobName, bool replace, bool useQueueAsync, int sasMinutes = 10, bool deleteOnFinish = false)
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
                useQueueAsync,
                Common.CalculateMD5(file.FullName)
            );
            Console.WriteLine($"File path:\t{uploader.FileUrl}");
            Console.WriteLine($"Relative path:\t{uploader.RelativeUrl}");

            // Upload file in different layer.
            var position = new Point(Console.CursorLeft, Console.CursorTop);
            TusClient.UploadFile(uploader.FileUrl, uploader.UploadToken.AccessToken, file, 5D, (transferred, total) =>
            {
                Console.SetCursorPosition(position.X, position.Y);
                Console.Write($"Progress:\t{(decimal)transferred / total:P2}\t\t{transferred}/{total}");
            });
            Console.WriteLine();
            Console.WriteLine($"Elapsed time:\t{stw.Elapsed}"); ;

            // Print result.
            PrintResult(client, uploader.FileUrl, file, sasMinutes);

            // Delete blob.
            if (deleteOnFinish)
            {
                Delete(client, uploader.FileUrl);
            }
        }

        static void TestModelo3_ByStream(TusSettings settings, string storeName, string containerName, System.IO.FileInfo file, string blobName, bool replace, bool useQueueAsync, int sasMinutes = 10, bool deleteOnFinish = false)
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
                new CreateFileOptions
                {
                    ContentType = Common.CalculateMimeType(file.FullName),
                    ContentTypeAuto = Common.SetContentTypeAuto(),
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
                var position = new Point(Console.CursorLeft, Console.CursorTop);
                TusClient.UploadFile(uploader.FileUrl, uploader.UploadToken.AccessToken, stream, 5D, (transferred, total) =>
                {
                    Console.SetCursorPosition(position.X, position.Y);
                    Console.Write($"Progress:\t{(decimal)transferred / total:P2}\t\t{transferred}/{total}");
                });
                Console.WriteLine();
                Console.WriteLine($"Elapsed time:\t{stw.Elapsed}"); ;
            }

            // Print result.
            PrintResult(client, uploader.FileUrl, file, sasMinutes);

            // Delete blob.
            if (deleteOnFinish)
            {
                Delete(client, uploader.FileUrl);
            }
        }

        static void PrintResult(TusClient client, string fileUrl, FileInfo file, int sasMinutes)
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
                details = client.GetFileDetails(fileUrl, includeVersions: true);
                Console.WriteLine($"Status: {details.Status?.ToString() ?? "(null)"}; Percentaje: {details.UploadPercentage?.ToString("P") ?? "(null)"}");
            }
            while (details.Status == UploadStatus.Uploading);
            Console.WriteLine();

            /* Generate SAS */
            Uri sasUri, sasUriInline, sasUriAttachment;
            sasUri = client.GenerateSasUrl(new Uri(fileUrl), TimeSpan.FromMinutes(sasMinutes));
            Console.WriteLine($"Created on:\t {details.CreatedOn.ToLocalTime()} ({details.CreatedOn})");
            Console.WriteLine($"Url SAS:\t{sasUri.OriginalString}");
            sasUriInline = sasUri.WithQueryValues(new Dictionary<string, string>() { { "inline", "true" } });
            Console.WriteLine($"        \t{sasUriInline.OriginalString}");
            sasUriAttachment = sasUri.WithQueryValues(new Dictionary<string, string>() { { "inline", "false" } });
            Console.WriteLine($"        \t{sasUriAttachment.OriginalString}");

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

                detailsWithVer = client.GetFileDetails(urlWithVer);
                sasUrlWithVer = client.GenerateSasUrl(urlWithVer, TimeSpan.FromMinutes(10));
                Console.WriteLine();
                Console.WriteLine("Previous version:");
                Console.WriteLine($"Created on:\t{detailsWithVer.CreatedOn}");
                Console.WriteLine($"Url SAS:\t{sasUrlWithVer}");
            }
        }

        static void Delete(TusClient client, string fileUrl)
        {
            Console.WriteLine();

            client.DeleteBlob(fileUrl);
            Console.WriteLine("Deleted.");
        }

    }
}