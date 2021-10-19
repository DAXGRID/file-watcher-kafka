using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Topos.Config;
using Topos.Producer;

namespace FileWatcherKafka
{
    public class FileWatcher : IFileWatcher, IDisposable
    {
        private readonly ILogger<FileWatcher> _logger;
        private FileSystemWatcher _watcher;
        private KafkaSetting _kafkaSetting;
        private IToposProducer _producer;

        public FileWatcher(ILogger<FileWatcher> logger, IOptions<KafkaSetting> kafkaSetting)
        {
            _logger = logger;
            _watcher = new FileSystemWatcher("/home/notation/test");
            _kafkaSetting = kafkaSetting.Value;
            _producer = Configure
                .Producer(c => c.UseKafka(_kafkaSetting.Server))
                .Serialization(s => s.UseNewtonsoftJson())
                .Create();
        }

        public void Start()
        {
            _watcher.Changed += async (s, e) => await OnChanged(s, e);
            _watcher.Error += OnError;
            _watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            _watcher.Dispose();
            _producer.Dispose();
        }

        private async Task OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
                return;

            var sha256CheckSum = SHA256CheckSum(e.FullPath);
            if (string.IsNullOrEmpty(sha256CheckSum))
                throw new Exception($"File '{e.FullPath}' SHA256Checksum cannot be null or empty.");

            _logger.LogInformation($"Changed: {e.FullPath}, publishing to Kafka with SHA256CheckSum {sha256CheckSum}.");
            await _producer.Send(_kafkaSetting.Topic, new ToposMessage(new FileChangedEvent(e.FullPath, sha256CheckSum)));
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
