using System;
using App.Metrics;
using App.Metrics.AspNetCore;
using JetBrains.Annotations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace DeckHubLive
{
    public class Program
    {
        [UsedImplicitly]
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStandardMetrics()
                .UseStartup<Startup>()
                .Build();
    }
}
