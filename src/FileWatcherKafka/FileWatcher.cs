using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Topos.Config;
using Topos.Producer;

namespace FileWatcherKafka
{
    public class FileWatcher : IFileWatcher, IDisposable
    {
        private readonly ILogger<FileWatcher> _logger;
        private readonly FileSystemWatcher _watcher;
        private readonly KafkaSetting _kafkaSetting;
        private readonly IToposProducer _producer;
        private readonly WatchSetting _watchSetting;

        public FileWatcher(
            ILogger<FileWatcher> logger,
            IOptions<KafkaSetting> kafkaSetting,
            IOptions<WatchSetting> watchSetting)
        {
            _logger = logger;
            _kafkaSetting = kafkaSetting.Value;
            _watchSetting = watchSetting.Value;

            if (string.IsNullOrWhiteSpace(_watchSetting.Directory))
                throw new ArgumentException($"{nameof(_watchSetting.Directory)} cannot be null, empty or whitespace.");

            if (string.IsNullOrWhiteSpace(_kafkaSetting.Server) ||
                string.IsNullOrWhiteSpace(_kafkaSetting.Consumer) ||
                string.IsNullOrWhiteSpace(_kafkaSetting.Topic))
                throw new ArgumentException($"All values in {nameof(kafkaSetting)} must not be null, empty or whitespace");

            _watcher = new FileSystemWatcher(_watchSetting.Directory);

            _producer = Configure
                .Producer(c => c.UseKafka(_kafkaSetting.Server))
                .Serialization(s => s.UseNewtonsoftJson())
                .Create();
        }

        public void Dispose()
        {
            _watcher.Dispose();
            _producer.Dispose();
        }

        public void Start()
        {
            _watcher.Error += OnError;
            Observable.FromEventPattern<FileSystemEventArgs>(_watcher, "Changed")
                        .Throttle(new TimeSpan(2500000))
                        .Subscribe(OnChanged);
            _watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(EventPattern<FileSystemEventArgs> obj)
        {
            var (sender, e) = obj;
            if (e == null)
                return;

            if (e.ChangeType != WatcherChangeTypes.Changed)
                return;

            var sha256CheckSum = SHA256CheckSum(e.FullPath);
            if (string.IsNullOrEmpty(sha256CheckSum))
                throw new Exception($"File '{e.FullPath}' SHA256Checksum cannot be null or empty.");

            _logger.LogInformation($"Changed: {e.FullPath}, publishing to Kafka with SHA256CheckSum {sha256CheckSum}.");

            if (string.IsNullOrWhiteSpace(_watchSetting.Directory))
                throw new ArgumentException($"{nameof(_watchSetting.Directory)} cannot be null, empty or whitespace.");

            // We replace it here so only from the current path from fileserver.
            var fullPath = e.FullPath.Replace(_watchSetting.Directory, "");

            _producer.Send(_kafkaSetting.Topic, new ToposMessage(new FileChangedEvent(fullPath, sha256CheckSum))).Wait();
        }

        private void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private void PrintException(Exception? ex)
        {
            if (ex is not null)
                _logger.LogError($"Message: {ex.Message}, exception {ex.StackTrace}");
        }

        private string SHA256CheckSum(string filePath)
        {
            using (var SHA256 = SHA256Managed.Create())
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                    return Convert.ToBase64String(SHA256.ComputeHash(fileStream));
            }
        }
    }
}
