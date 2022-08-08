// See https://aka.ms/new-console-template for more information

const string ServerAddress = "localhost";
const int ServerPort = 5001;

var serverUrl = string.Format("https://{0}:{1}/files/", ServerAddress, ServerPort);
var file = new FileInfo(@"C:\Users\hfrances\Downloads\multipass-1.9.0+win-win64.exe");
var stw = System.Diagnostics.Stopwatch.StartNew();

var client = new TusDotNetClient.TusClient();
var fileUrl = await client.CreateAsync(serverUrl, file, new (string key, string value)[] {
   new("container", "default"),
   new("factor", "1,2")
});
var uploadOperation = client.UploadAsync(fileUrl, file, chunkSize: 5D);

uploadOperation.Progressed += (transferred, total) =>
    Console.WriteLine($"Progress: {transferred}/{total}");

await uploadOperation;
Console.WriteLine($"Elapsed time: {stw.Elapsed} - {fileUrl}");
Console.WriteLine();
Console.ReadKey();