using System;

namespace FileWatcherKafka
{
    public interface IFileWatcher : IDisposable
    {
        void Start();
    }
}
