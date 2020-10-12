using AutomationNodes;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace AutomationApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var worldTime = host.Services.GetService(typeof(WorldTime)) as WorldTime;
            var token = host.Services.GetService(typeof(ApplicationRunningToken)) as ApplicationRunningToken;
            var worldCatalogue = host.Services.GetService(typeof(WorldCatalogue)) as WorldCatalogue;
            Task.Run(() => worldCatalogue.StartTemporalEventQueue(token.CancellationToken.Token));

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
