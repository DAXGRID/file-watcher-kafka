using System;

namespace FileWatcherKafka
{
    public record FileChangedEvent
    {
        public Guid EventId { get; init; }
        public string EventType { get; init; }
        public DateTime EventTimeStamp { get; init; }
        public string FullPath { get; init; }

        public FileChangedEvent(string fullPath)
        {
            EventId = Guid.NewGuid();
            EventTimeStamp = DateTime.UtcNow;
            EventType = nameof(FileChangedEvent);
            FullPath = fullPath;
        }
    }
}
