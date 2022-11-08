using System.IO;

namespace TusWebApplication.TusAzure
{
    sealed class QueueItem
    {
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

    }
}
