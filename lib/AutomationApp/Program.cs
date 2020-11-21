using AutomationApp.Hubs;
using AutomationNodes.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace AutomationApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            StartTemporalEventQueue(host);
            StartHubManager(host);

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void StartTemporalEventQueue(IHost host)
        {
            var worldTime = host.Services.GetService(typeof(WorldTime)) as WorldTime;
            var token = host.Services.GetService(typeof(ApplicationRunningToken)) as ApplicationRunningToken;
            var temporalEventQueue = host.Services.GetService(typeof(ITemporalEventQueue)) as TemporalEventQueue;
            Task.Run(() => temporalEventQueue.StartTemporalEventQueue(token.CancellationToken.Token));
        }

        private static void StartHubManager(IHost host)
        {
            var token = host.Services.GetService(typeof(ApplicationRunningToken)) as ApplicationRunningToken;
            var hubManager = host.Services.GetService(typeof(IHubManager)) as HubManager;
            Task.Run(() => hubManager.Start(token.CancellationToken.Token));
        }
    }
}
