using System;
using System.Collections.Generic;

namespace TusWebApplication.TusAzure
{
    sealed class BlobManager : IBlobManager
    {

        TusAzureStoreDictionary TusAzureStores { get; }


        public BlobManager(TusAzureStoreDictionary tusAzureStores)
        {
            this.TusAzureStores = tusAzureStores;
        }

        public BlobStatus? GetBlobStatus(string storeName, string container, string blobName)
        {
            BlobStatus? rdo;

            if (TusAzureStores.TryGetValue(storeName, out TusAzureStore? store))
            {
                string blobId = $"{container}/{blobName}";

                if (store.Blobs.TryGetValue(blobId, out BlobInfo? blobInfo))
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
            }
            else
            {
                throw new KeyNotFoundException($"Store not found: '{storeName}'.");
            }
            return rdo;
        }

    }
}
