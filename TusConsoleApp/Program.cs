// See https://aka.ms/new-console-template for more information

using qckdev;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Principal;

var commandArgs = qckdev.CommandArgsDictionary.Create(args);

if (commandArgs.TryGetValue("address", out string? serverUrl))
{
    var containerName = commandArgs.GetValueOrDefault("container", string.Empty);
    var blobName = commandArgs.GetValueOrDefault("blob", string.Empty);
    var replace = commandArgs.GetValueOrDefault("replace", "false").In("", "true");

    if (commandArgs.TryGetValue("0", out string? fileName))
    {
        var file = new FileInfo(fileName);

        if (file.Exists)
        {
            Thread.Sleep(2000); // Esperar a que cargue el servidor.

            /* Upload file */
            var stw = System.Diagnostics.Stopwatch.StartNew();
            var client = new TusDotNetClient.TusClient();
            var fileUrl = await client.CreateAsync(serverUrl, file, new (string key, string value)[] {
               new("BLOB:container", containerName),
               new("BLOB:name", blobName),
               new("BLOB:replace", replace.ToString()),
               new("TAG:extension", file.Extension),
               new("factor", "1,2")
            });
            var position = Console.GetCursorPosition();
            var uploadOperation = client.UploadAsync(fileUrl, file, chunkSize: 5D);

            uploadOperation.Progressed += (transferred, total) =>
            {
                Console.SetCursorPosition(position.Left, position.Top);
                Console.Write($"Progress:\t{(decimal)transferred / total:P2}\t\t{transferred}/{total}");
            };
            await uploadOperation;
            Console.WriteLine();

            /* Calculate Hash */
            string contentHash;
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(file.FullName))
                {
                    contentHash = Convert.ToBase64String(md5.ComputeHash(stream));
                }
            }

            /* Output */
            Console.WriteLine($"Elapsed time:\t{stw.Elapsed}");
            Console.WriteLine($"File:\t\t{fileUrl}");
            Console.WriteLine($"Hash:\t\t{contentHash}");
        }
        else
        {
            Console.WriteLine("File not file not found.");
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