// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography;

const string ServerAddress = "localhost";
const int ServerPort = 5000;


System.Threading.Thread.Sleep(2000);

var serverUrl = string.Format("http://{0}:{1}/files/", ServerAddress, ServerPort);
var file = new FileInfo(@"C:\Users\hfrances\Downloads\Docker Desktop Installer.exe");

/* Upload file */
var stw = System.Diagnostics.Stopwatch.StartNew();
var client = new TusDotNetClient.TusClient();
var fileUrl = await client.CreateAsync(serverUrl, file, new (string key, string value)[] {
   new("container", "other"),
   new("factor", "1,2")
});
var uploadOperation = client.UploadAsync(fileUrl, file, chunkSize: 5D);

uploadOperation.Progressed += (transferred, total) =>
    Console.WriteLine($"Progress: {transferred}/{total}");

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
Console.WriteLine();
//Console.ReadKey();