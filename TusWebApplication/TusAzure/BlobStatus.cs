namespace TusWebApplication.TusAzure
{
    sealed class BlobStatus
    {

        public enum UploadStatus
        {
            Unknown,
            Uploading,
            Done,
            Error
        }

        public string BlobId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public long Length { get; set; }
        public UploadStatus Status { get; set; }
        public string? ErrorDescripton { get; set; }
        public int LocalChunks { get; set; }
        public long LocalLength { get; set; }
        public int RemoteChunks { get; set; }
        public long RemoteLength { get; set; }
        public double LocalPercentage { get; set; }
        public double RemotePercentage { get; set; }

    }
}
