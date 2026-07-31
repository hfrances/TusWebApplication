using System.Threading;
using System.Threading.Tasks;

namespace TusWebApplication.TusAzure
{
    interface IBlobManager
    {

        Task<BlobStatus?> GetBlobStatusAsync(string storeName, string container, string blobName, CancellationToken cancellationToken);

    }
}
