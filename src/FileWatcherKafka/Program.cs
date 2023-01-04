using FileWatcherKafka.Config;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Threading.Tasks;

namespace FileWatcherKafka;

public class Program
{
    async static Task Main(string[] args)
    {
        var root = Directory.GetCurrentDirectory();
        var dotenv = Path.Combine(root, ".env");
        DotEnv.Load(dotenv);

        using var host = HostConfig.Configure();
        await host.StartAsync();
        await host.WaitForShutdownAsync();
    }
}
