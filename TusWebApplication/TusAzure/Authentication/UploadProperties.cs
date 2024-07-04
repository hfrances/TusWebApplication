using System;

namespace TusWebApplication.TusAzure.Authentication
{
    sealed class UploadProperties
    {

        public string Container { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string Blob { get; set; } = string.Empty;
        public string BlobId { get; set; } = string.Empty;
        public string? ContentType { get; set; }
        public bool? ContentTypeAuto { get; set; }
        public string? ContentLanguage { get; set; }
        public bool Replace { get; set; }
        public long Size { get; set; }
        public string? Hash { get; set; }
        public bool UseQueueAsync { get; set; }
        public DateTimeOffset FirstRequestExpired { get; set; }

    }
}
