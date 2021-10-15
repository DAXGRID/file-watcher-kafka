using System;

namespace FileWatcherKafka
{
    public record FileChangedEvent
    {
        public string EventType { get; init; }
        public DateTime EventTimeStamp { get; init; }
        public string FullPath { get; init; }

        public FileChangedEvent(string fullPath)
        {
            EventTimeStamp = DateTime.UtcNow;
            EventType = nameof(FileChangedEvent);
            FullPath = fullPath;
        }
    }
}
