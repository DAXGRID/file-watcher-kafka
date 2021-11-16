using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using FileWatcherKafka.Config;

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
