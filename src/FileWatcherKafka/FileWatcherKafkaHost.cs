using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileWatcherKafka;

public class FileWatcherKafkaHost : IHostedService
{
    private readonly ILogger<FileWatcherKafkaHost> _logger;
    private readonly IHostApplicationLifetime _hostApplicationLifeTime;
    private readonly IFileWatcher _fileWatcher;

    public FileWatcherKafkaHost(
        ILogger<FileWatcherKafkaHost> logger,
        IHostApplicationLifetime hostApplicationLifetime,
        IFileWatcher fileWatcher)
    {
        _logger = logger;
        _hostApplicationLifeTime = hostApplicationLifetime;
        _fileWatcher = fileWatcher;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Starting {nameof(FileWatcherKafkaHost)}.");
        _hostApplicationLifeTime.ApplicationStarted.Register(OnStarted);
        _hostApplicationLifeTime.ApplicationStopping.Register(OnStopped);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private void MarkAsReady()
    {
        File.Create("/tmp/healthy");
    }

    private void OnStarted()
    {
        _fileWatcher.Start();
        MarkAsReady();
    }

    private void OnStopped()
    {
        try
        {
            _fileWatcher.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Message: {ex.Message}, exception {ex.StackTrace}");
        }
        _logger.LogInformation("Stopped");
    }
}
