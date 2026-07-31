using System.Threading;
using System.Threading.Tasks;

namespace TusWebApplication.TusAzure
{
    interface IBlobUploadStore
    {

        Task<bool> TryAddAsync(string storeName, BlobInfo blobInfo, CancellationToken cancellationToken);

        Task<BlobInfo?> GetAsync(string storeName, string blobId, CancellationToken cancellationToken);

        Task<bool> TryRemoveAsync(string storeName, string blobId, CancellationToken cancellationToken);

    }
}
