using Azure.Storage.Blobs.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TusWebApplication.TusAzure;

namespace TusClientLibrary.Test
{
    [TestClass]
    public sealed class BlobUploadMemoryStoreTests
    {

        const string STORE_NAME = "mystore";


        [TestMethod]
        public async Task TryAddAsync_Concurrent()
        {
            var store = new BlobUploadMemoryStore();
            var blobs = Enumerable.Range(0, 256)
                .Select(index => CreateBlobInfo($"container/blob-{index}"))
                .ToArray();

            try
            {
                var results = await Task.WhenAll(blobs.Select(blobInfo =>
                    Task.Run(async () => await store.TryAddAsync(STORE_NAME, blobInfo, CancellationToken.None))
                ));

                Assert.IsTrue(results.All(result => result));
                foreach (var blobInfo in blobs)
                {
                    Assert.AreSame(
                        blobInfo,
                        await store.GetAsync(STORE_NAME, blobInfo.FileId, CancellationToken.None)
                    );
                }
            }
            finally
            {
                foreach (var blobInfo in blobs)
                {
                    await store.TryRemoveAsync(STORE_NAME, blobInfo.FileId, CancellationToken.None);
                    blobInfo.Dispose();
                }
            }
        }

        [TestMethod]
        public async Task TryAddAsync_ConcurrentSameBlob()
        {
            var store = new BlobUploadMemoryStore();
            var blobInfo = CreateBlobInfo("container/blob");

            try
            {
                var results = await Task.WhenAll(Enumerable.Range(0, 256).Select(_ =>
                    Task.Run(async () => await store.TryAddAsync(STORE_NAME, blobInfo, CancellationToken.None))
                ));

                Assert.AreEqual(1, results.Count(result => result));
                Assert.AreSame(
                    blobInfo,
                    await store.GetAsync(STORE_NAME, blobInfo.FileId, CancellationToken.None)
                );
            }
            finally
            {
                await store.TryRemoveAsync(STORE_NAME, blobInfo.FileId, CancellationToken.None);
                blobInfo.Dispose();
            }
        }

        [TestMethod]
        public async Task GetAndRemoveAsync_Concurrent()
        {
            var store = new BlobUploadMemoryStore();
            var blobs = Enumerable.Range(0, 256)
                .Select(index => CreateBlobInfo($"container/blob-{index}"))
                .ToArray();

            foreach (var blobInfo in blobs)
            {
                await store.TryAddAsync(STORE_NAME, blobInfo, CancellationToken.None);
            }

            try
            {
                var readTask = Task.Run(async () =>
                {
                    for (int iteration = 0; iteration < 10; iteration++)
                    {
                        foreach (var blobInfo in blobs)
                        {
                            await store.GetAsync(STORE_NAME, blobInfo.FileId, CancellationToken.None);
                        }
                    }
                });
                var removeTasks = blobs.Select(blobInfo =>
                    Task.Run(async () => await store.TryRemoveAsync(STORE_NAME, blobInfo.FileId, CancellationToken.None))
                );
                var results = await Task.WhenAll(removeTasks);

                await readTask;
                Assert.IsTrue(results.All(result => result));
                foreach (var blobInfo in blobs)
                {
                    Assert.IsNull(
                        await store.GetAsync(STORE_NAME, blobInfo.FileId, CancellationToken.None)
                    );
                }
            }
            finally
            {
                foreach (var blobInfo in blobs)
                {
                    blobInfo.Dispose();
                }
            }
        }

        [TestMethod]
        public async Task GetAsync_KeyComparison()
        {
            var store = new BlobUploadMemoryStore();
            var blobInfo = CreateBlobInfo("container/MyBlob");

            try
            {
                Assert.IsTrue(await store.TryAddAsync("MyStore", blobInfo, CancellationToken.None));
                Assert.AreSame(
                    blobInfo,
                    await store.GetAsync("mystore", "container/MyBlob", CancellationToken.None)
                );
                Assert.IsNull(
                    await store.GetAsync("mystore", "container/myblob", CancellationToken.None)
                );
            }
            finally
            {
                await store.TryRemoveAsync("MYSTORE", blobInfo.FileId, CancellationToken.None);
                blobInfo.Dispose();
            }
        }

        [TestMethod]
        public async Task GetBlobStatusAsync()
        {
            var store = new BlobUploadMemoryStore();
            var manager = new BlobManager(store);
            var blobInfo = CreateBlobInfo("container/blob", uploadLength: 100);

            blobInfo.QueuePosition = 2;
            blobInfo.QueueCount = 3;
            blobInfo.SizeOffset = 75;
            blobInfo.SizeOffsetInternal = 50;

            try
            {
                Assert.IsTrue(await store.TryAddAsync(STORE_NAME, blobInfo, CancellationToken.None));

                var status = await manager.GetBlobStatusAsync(STORE_NAME, "container", "blob", CancellationToken.None);
                Assert.IsNotNull(status);
                Assert.AreEqual(BlobStatus.UploadStatus.Uploading, status.Status);
                Assert.AreEqual(2, status.LocalChunks);
                Assert.AreEqual(3, status.RemoteChunks);
                Assert.AreEqual(75, status.LocalLength);
                Assert.AreEqual(50, status.RemoteLength);
                Assert.AreEqual(0.5D, status.RemotePercentage);

                blobInfo.Done = true;
                status = await manager.GetBlobStatusAsync(STORE_NAME, "container", "blob", CancellationToken.None);
                Assert.IsNotNull(status);
                Assert.AreEqual(BlobStatus.UploadStatus.Done, status.Status);

                blobInfo.Error = new InvalidOperationException("Upload failed.");
                status = await manager.GetBlobStatusAsync(STORE_NAME, "container", "blob", CancellationToken.None);
                Assert.IsNotNull(status);
                Assert.AreEqual(BlobStatus.UploadStatus.Error, status.Status);
                Assert.AreEqual(blobInfo.Error.Message, status.ErrorDescripton);
            }
            finally
            {
                await store.TryRemoveAsync(STORE_NAME, blobInfo.FileId, CancellationToken.None);
                blobInfo.Dispose();
            }
        }

        static BlobInfo CreateBlobInfo(string fileId, long uploadLength = 1)
        {
            var fileIdParts = fileId.Split('/');
            var blob = new BlockBlobClient(new Uri($"https://account.blob.core.windows.net/{fileId}"));

            return new BlobInfo(
                fileId,
                fileIdParts[0],
                fileIdParts[1],
                fileIdParts[1],
                string.Empty,
                uploadLength,
                false,
                blob
            );
        }

    }
}
