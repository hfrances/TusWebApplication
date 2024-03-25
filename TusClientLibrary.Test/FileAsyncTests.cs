using Microsoft.VisualStudio.TestTools.UnitTesting;
using qckdev.Net.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using TusClientLibrary.Test.Models;

namespace TusClientLibrary.Test
{
    [TestClass]
    public class FileAsyncTests : Base.BaseTests
    {

        const string BLOB_STORE_NAME = "hfcloud_eu";
        const string BLOB_CONTAINER_NAME = "other";

        [TestMethod]
        public async Task CreateAndRemoveFileAsync()
        {
            var blobName = Guid.NewGuid().ToString();
            var fileName = System.IO.Path.GetTempFileName();
            using var stream = new StreamWriter(fileName);

            try
            {
                stream.Write("This is a disk file test.");
                stream.Close();

                var tusClient = CreateTusClient();
                var fileInfo = new System.IO.FileInfo(fileName);
                CreateFileResponse newFile;

                newFile = await CreateFileAsync(tusClient, BLOB_STORE_NAME, BLOB_CONTAINER_NAME, blobName, fileInfo);
                await DeleteFileAsync(tusClient, newFile.FileUrl);
            }
            finally
            {
                System.IO.File.Delete(fileName);
            }
        }

        [TestMethod]
        public async Task GetFileDetailsAsync()
        {
            var blobName = Guid.NewGuid().ToString();
            var fileName = "file1.txt";
            using var fileContent = CreateStream("This is my first file");

            await TestBlob(BLOB_STORE_NAME, BLOB_CONTAINER_NAME, blobName, fileName, fileContent, async (tusClient, newFile) =>
            {
                FileDetails fileDetails;

                fileDetails = await tusClient.GetFileDetailsAsync(newFile.FileUrl);
                fileDetails.InnerUrl.ToString();
            });
        }

        [TestMethod]
        public async Task GetFileDetailsAsync_RelativeUrl()
        {
            var blobName = Guid.NewGuid().ToString();
            var fileName = "file1.txt";
            using var fileContent = CreateStream("This is my first file");

            await TestBlob(BLOB_STORE_NAME, BLOB_CONTAINER_NAME, blobName, fileName, fileContent, async (tusClient, newFile) =>
            {
                FileDetails fileDetails;

                fileDetails = await tusClient.GetFileDetailsAsync(newFile.RelativeUrl);
                fileDetails.InnerUrl.ToString();
            });
        }

        [TestMethod]
        public async Task GetFileDetailsAsync_Version()
        {
            var blobName = Guid.NewGuid().ToString();
            var fileName = "file1.txt";
            using var fileContent = CreateStream("This is my first file");

            await TestBlob(BLOB_STORE_NAME, BLOB_CONTAINER_NAME, blobName, fileName, fileContent, async (tusClient, newFile) =>
            {
                FileDetails fileDetails, fileDetailsVersion;

                fileDetails = await tusClient.GetFileDetailsAsync(newFile.RelativeUrl, includeVersions: true);
                fileDetailsVersion = await tusClient.GetFileDetailsAsync(newFile.RelativeUrl, fileDetails.Versions.Last().VersionId);
                fileDetails.InnerUrl.ToString();
            });
        }

        [TestMethod]
        public async Task GenerateTokenSas()
        {
            var blobName = Guid.NewGuid().ToString();
            var fileName = "file1.txt";
            using var fileContent = CreateStream("This is my first file");

            await TestBlob(BLOB_STORE_NAME, BLOB_CONTAINER_NAME, blobName, fileName, fileContent, async (tusClient, newFile) =>
            {
                string urlSas;

                urlSas = await tusClient.GenerateSasUrlAsync(newFile.FileUrl, TimeSpan.FromMinutes(10));
                await HttpClient.GetAsync(urlSas);
            });
        }

        [TestMethod]
        public async Task GenerateTokenSas_Version_RelativeUrl()
        {
            var blobName = Guid.NewGuid().ToString();
            var fileName = "file1.txt";
            using var fileContent = CreateStream("This is my first file");

            await TestBlob(BLOB_STORE_NAME, BLOB_CONTAINER_NAME, blobName, fileName, fileContent, async (tusClient, newFile) =>
            {
                string urlSas;

                urlSas = await tusClient.GenerateSasUrlAsync(newFile.RelativeUrl, TimeSpan.FromMinutes(10));
                await HttpClient.GetAsync(urlSas);
            });
        }

        [TestMethod]
        public async Task GenerateTokenSas_Version()
        {
            var blobName = Guid.NewGuid().ToString();
            var fileName = "file1.txt";
            using var fileContent = CreateStream("This is my first file");

            await TestBlob(BLOB_STORE_NAME, BLOB_CONTAINER_NAME, blobName, fileName, fileContent, async (tusClient, newFile) =>
            {
                FileDetails fileDetails;
                string versionId;
                string urlWithVersion;
                string urlSas;

                fileDetails = await tusClient.GetFileDetailsAsync(newFile.FileUrl, includeVersions: true);
                versionId = fileDetails.Versions.Last()?.VersionId;
                urlWithVersion = (new Uri(newFile.FileUrl)).WithQueryValues(new Dictionary<string, string> { { "versionId", versionId } }).ToString();

                urlSas = await tusClient.GenerateSasUrlAsync(urlWithVersion, TimeSpan.FromMinutes(10));
                await HttpClient.GetAsync(urlSas);
            });
        }

        [TestMethod]
        public async Task GenerateTokenSasAsync_Multiple()
        {
            var list = new List<(string BlobName, string FileName, Stream FileContent)>
            {
                (null, "file1.txt", CreateStream("This is my first file")),
                (null, "file2.txt", CreateStream("This is my second file"))
            };

            try
            {
                await TestBlobs(BLOB_STORE_NAME, BLOB_CONTAINER_NAME, list, async (tusClient, newFiles) =>
                {
                    var urls = newFiles.Select(x => x.FileUrl);
                    IEnumerable<TokenSas> urlsSas;

                    urlsSas = await tusClient.GenerateSasUrlAsync(urls, TimeSpan.FromMinutes(10));
                    urlsSas.ToString();
                });
            }
            finally
            {
                list.ForEach(x => x.FileContent.Dispose());
                list.Clear();
            }
        }

        [TestMethod]
        public async Task GenerateTokenSasAsync_Multiple_Version()
        {
            var list = new List<(string, string FileName, Stream FileContent)>
            {
                (null, "file1.txt", CreateStream("This is my first file")),
                (null, "file2.txt", CreateStream("This is my second file"))
            };

            try
            {
                await TestBlobs(BLOB_STORE_NAME, BLOB_CONTAINER_NAME, list, async (tusClient, newFiles) =>
                {
                    var urls = newFiles.Select(x => x.FileUrl).ToArray();
                    IEnumerable<TokenSas> urlsSas;

                    for (int i = 0; i < urls.Length; i++)
                    {
                        var url = urls[i];
                        FileDetails fileDetails;
                        string versionId;
                        string urlWithVersion;

                        fileDetails = await tusClient.GetFileDetailsAsync(url, includeVersions: true);
                        versionId = fileDetails.Versions.Last()?.VersionId;
                        urlWithVersion = (new Uri(url)).WithQueryValues(new Dictionary<string, string> { { "versionId", versionId } }).ToString();
                        urls[i] = urlWithVersion;
                    }

                    urlsSas = await tusClient.GenerateSasUrlAsync(urls, TimeSpan.FromMinutes(10));
                    urlsSas.ToString();
                });
            }
            finally
            {
                list.ForEach(x => x.FileContent.Dispose());
                list.Clear();
            }
        }


        [TestMethod]
        public async Task GenerateTokenSasAsync_Multiple_RelativeUrl()
        {
            var list = new List<(string BlobName, string FileName, Stream FileContent)>
            {
                (null, "file1.txt", CreateStream("This is my first file")),
                (null, "file2.txt", CreateStream("This is my second file"))
            };

            try
            {
                await TestBlobs(BLOB_STORE_NAME, BLOB_CONTAINER_NAME, list, async (tusClient, newFiles) =>
                {
                    var urls = newFiles
                        .Select(x => x.RelativeUrl)
                        .Append($"files/{BLOB_STORE_NAME}/{BLOB_CONTAINER_NAME}/8960453D-58CD-47D8-8FC3-B21426BF4DE8"); // Inexistent
                    IEnumerable<TokenSas> urlsSas;

                    urlsSas = await tusClient.GenerateSasUrlAsync(urls, TimeSpan.FromMinutes(10));
                    urlsSas.ToArray();
                });
            }
            finally
            {
                list.ForEach(x => x.FileContent.Dispose());
                list.Clear();
            }
        }



        private TusClient CreateTusClient()
        {
            return new TusClient(
                HttpClient,
                new TusClientCredentials
                {
                    UserName = Settings.Security.Credentials.UserName,
                    Login = Settings.Security.Credentials.Login,
                    Password = Settings.Security.Credentials.Password
                }
            );
        }

        private static async Task<CreateFileResponse> CreateFileAsync(TusClient tusClient, string storeName, string containerName, string blobName, FileInfo fileInfo)
        {
            TusUploaderAsync uploader;

            uploader = await tusClient.CreateFileAsync(fileInfo, storeName, containerName, blobName, options: null);
            await uploader.UploadAsync(fileInfo);
            return new CreateFileResponse
            {
                FileUrl = uploader.FileUrl,
                RelativeUrl = uploader.RelativeUrl
            };
        }

        private static async Task<CreateFileResponse> CreateFileAsync(TusClient tusClient, string storeName, string containerName, string blobName, string fileName, Stream content)
        {
            TusUploaderAsync uploader;

            uploader = await tusClient.CreateFileAsync(storeName, containerName, fileName, content.Length, blobName, options: null);
            await uploader.UploadAsync(content);
            return new CreateFileResponse
            {
                FileUrl = uploader.FileUrl,
                RelativeUrl = uploader.RelativeUrl
            };
        }

        private static Task DeleteFileAsync(TusClient tusClient, string url)
        {
            return tusClient.DeleteBlobAsync(url);
        }

        private async Task TestBlob(string storeName, string containerName, string blobName, FileInfo fileInfo, Func<TusClient, CreateFileResponse, Task> predicate)
        {
            var tusClient = CreateTusClient();
            CreateFileResponse response;

            response = await CreateFileAsync(tusClient, storeName, containerName, blobName, fileInfo);
            try
            {
                await predicate(tusClient, response);
            }
            finally
            {
                await DeleteFileAsync(tusClient, response.FileUrl);
            }
        }

        private async Task TestBlob(string storeName, string containerName, string blobName, string fileName, Stream content, Func<TusClient, CreateFileResponse, Task> predicate)
        {
            var tusClient = CreateTusClient();
            CreateFileResponse response;

            response = await CreateFileAsync(tusClient, storeName, containerName, blobName, fileName, content);
            try
            {
                await predicate(tusClient, response);
            }
            finally
            {
                await DeleteFileAsync(tusClient, response.FileUrl);
            }
        }

        private async Task TestBlobs(string storeName, string containerName, IEnumerable<(string BlobName, string FileName, Stream FileContent)> fileInfos, Func<TusClient, IEnumerable<CreateFileResponse>, Task> predicate)
        {
            var tusClient = CreateTusClient();
            var list = new List<CreateFileResponse>();

            try
            {
                CreateFileResponse response;

                foreach (var (blobName, fileName, fileContent) in fileInfos)
                {
                    response = await CreateFileAsync(tusClient, storeName, containerName, blobName, fileName, fileContent);
                    list.Add(response);
                }
                await predicate(tusClient, list);
            }
            finally
            {
                foreach (var item in list)
                {
                    await DeleteFileAsync(tusClient, item.FileUrl);
                }
            }
        }


        private static Stream CreateStream(string content)
        {
            var stream = new MemoryStream();

            // Crear un StreamWriter para el MemoryStream
            using (StreamWriter writer = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
            {
                // Escribir una cadena en el MemoryStream
                writer.Write(content);
                writer.Flush();
            }

            // Restablecer la posición del MemoryStream
            stream.Position = 0;

            return stream;
        }

    }
}
