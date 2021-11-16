using System;

namespace FileWatcherKafka;
public record FileChangedEvent
{
    public Guid EventId { get; init; }
    public string EventType { get; init; }
    public DateTime EventTimeStamp { get; init; }
    public string FullPath { get; init; }
    public string Sha256CheckSum { get; init; }

    public FileChangedEvent(string fullPath, string sha256Checksum)
    {
        EventId = Guid.NewGuid();
        EventTimeStamp = DateTime.UtcNow;
        EventType = nameof(FileChangedEvent);
        FullPath = fullPath;
        Sha256CheckSum = sha256Checksum;
    }
}
