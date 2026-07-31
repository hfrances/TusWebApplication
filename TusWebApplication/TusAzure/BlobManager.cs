using System;
using System.Threading;
using System.Threading.Tasks;

namespace TusWebApplication.TusAzure
{
    sealed class BlobManager : IBlobManager
    {

        IBlobUploadStore BlobUploadStore { get; }


        public BlobManager(IBlobUploadStore blobUploadStore)
        {
            this.BlobUploadStore = blobUploadStore;
        }

        public async Task<BlobStatus?> GetBlobStatusAsync(string storeName, string container, string blobName, CancellationToken cancellationToken)
        {
            BlobStatus? rdo;
            string blobId = $"{container}/{blobName}";
            var blobInfo = await BlobUploadStore.GetAsync(storeName, blobId, cancellationToken);

            if (blobInfo != null)
            {
                rdo = new BlobStatus
                {
                    BlobId = blobInfo.FileId,
                    Name = blobInfo.FileName,
                    Length = blobInfo.UploadLength,
                    LocalChunks = blobInfo.QueuePosition,
                    LocalLength = blobInfo.SizeOffset,
                    RemoteChunks = blobInfo.QueueCount,
                    RemoteLength = blobInfo.SizeOffsetInternal,
                    RemotePercentage = Math.Round(blobInfo.SizeOffsetInternal * 1D / blobInfo.UploadLength, 2),
                };

                if (blobInfo.Done)
                {
                    if (blobInfo.Error == null)
                    {
                        rdo.Status = BlobStatus.UploadStatus.Done;
                    }
                    else
                    {
                        rdo.Status = BlobStatus.UploadStatus.Error;
                        rdo.ErrorDescripton = blobInfo.Error.Message;
                    }
                }
                else
                {
                    rdo.Status = BlobStatus.UploadStatus.Uploading;
                }
            }
            else
            {
                rdo = null; // BlobId not found.
            }
            return rdo;
        }

    }
}
