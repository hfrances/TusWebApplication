using System;
using System.IO;

namespace TusWebApplication.TusAzure
{
    sealed class QueueItem : IDisposable
    {
        private bool disposedValue;

        public string Name { get; }
        public Stream? Stream { get; set; }
        public long Length { get; set; }
        public QueueItemStatus Status { get; set; }

        public QueueItem(string name, Stream stream, long length)
        {
            Name = name;
            Stream = stream;
            Length = length;
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    Stream?.Dispose();
                    Stream = null;
                }

                // Free unmanaged resources (unmanaged objects) and override finalizer
                // Set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~QueueItem()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
