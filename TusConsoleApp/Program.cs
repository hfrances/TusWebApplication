// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography;

var commandArgs = qckdev.CommandArgsDictionary.Create(args);

if (commandArgs.TryGetValue("address", out string? serverUrl))
{

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
               new("container", "other"),
               new("META:factor", "1,2")
            });
            var uploadOperation = client.UploadAsync(fileUrl, file, chunkSize: 5D);

            uploadOperation.Progressed += (transferred, total) =>
                Console.WriteLine($"Progress: {(decimal)transferred / total:P2} {transferred}/{total}");

            await uploadOperation;

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
            Console.WriteLine($"Elapsed time: {stw.Elapsed} - {fileUrl} (Hash: {contentHash})");
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