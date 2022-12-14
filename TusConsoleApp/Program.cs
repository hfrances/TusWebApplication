using System;
using qckdev;
using System.Security.Cryptography;
using qckdev.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TusConsoleApp
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var commandArgs = qckdev.CommandArgsDictionary.Create(args);

            if (commandArgs.TryGetValue("address", out string serverUrl))
            {
                var containerName = commandArgs.TryGetValue("container", string.Empty);
                var blobName = commandArgs.TryGetValue("blob", string.Empty);
                var replace = commandArgs.TryGetValue("replace", "false").In("", "true");

                if (commandArgs.TryGetValue("0", out string fileName))
                {
                    var file = new FileInfo(fileName);

                    if (file.Exists)
                    {
                        Thread.Sleep(2000); // Esperar a que cargue el servidor.

                        /* Upload file */
                        var stw = System.Diagnostics.Stopwatch.StartNew();
                        var client = new TusDotNetClient.TusClient();
                        var fileUrl = await client.CreateAsync(serverUrl, file, new (string key, string value)[] {
                           ("BLOB:container", containerName),
                           ("BLOB:name", blobName),
                           ("BLOB:replace", replace.ToString()),
                           ("TAG:extension", file.Extension),
                           ("factor", "1,2")
                        });
                        (int Left, int Top) position = (Console.CursorLeft, Console.CursorTop);
                        var uploadOperation = client.UploadAsync(fileUrl, file, chunkSize: 5D);

                        uploadOperation.Progressed += (transferred, total) =>
                        {
                            Console.SetCursorPosition(position.Left, position.Top);
                            Console.Write($"Progress:\t{(decimal)transferred / total:P2}\t\t{transferred}/{total}");
                        };
                        await uploadOperation;
                        Console.WriteLine();
                        Console.WriteLine($"Elapsed time:\t{stw.Elapsed}");
                        Console.WriteLine($"File:\t\t{fileUrl}");

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
            //Console.ReadKey();
        }
    }
}