using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace TusWebApplication.TusAzure
{
    sealed class BlobUploadMemoryStore : IBlobUploadStore
    {

        ConcurrentDictionary<string, ConcurrentDictionary<string, BlobInfo>> Stores { get; }
            = new ConcurrentDictionary<string, ConcurrentDictionary<string, BlobInfo>>(StringComparer.OrdinalIgnoreCase);


        public Task<bool> TryAddAsync(string storeName, BlobInfo blobInfo, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var store = Stores.GetOrAdd(
                storeName,
                _ => new ConcurrentDictionary<string, BlobInfo>(StringComparer.Ordinal)
            );
            return Task.FromResult(store.TryAdd(blobInfo.FileId, blobInfo));
        }

        public Task<BlobInfo?> GetAsync(string storeName, string blobId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            BlobInfo? blobInfo = null;

            if (Stores.TryGetValue(storeName, out ConcurrentDictionary<string, BlobInfo>? store))
            {
                store.TryGetValue(blobId, out blobInfo);
            }
            return Task.FromResult(blobInfo);
        }

        public Task<bool> TryRemoveAsync(string storeName, string blobId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            bool rdo;

            if (Stores.TryGetValue(storeName, out ConcurrentDictionary<string, BlobInfo>? store))
            {
                rdo = store.TryRemove(blobId, out _);
            }
            else
            {
                rdo = false;
            }
            return Task.FromResult(rdo);
        }

    }
}
