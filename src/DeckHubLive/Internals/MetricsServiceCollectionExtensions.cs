using System;
using App.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace DeckHubLive
{
    public static class MetricsServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultMetrics(this IServiceCollection services,
            IConfiguration configuration, string defaultContextLabel)
        {

            var influxDb = configuration["AppMetrics:InfluxDbServer"];
            var influxDatabase = configuration["AppMetrics:InfluxDbDatabase"];
            if (!string.IsNullOrWhiteSpace(influxDb))
            {
                var metrics = AppMetrics.CreateDefaultBuilder()
                    .Report.ToInfluxDb(influxDb, influxDatabase, TimeSpan.FromSeconds(5))
                    .Configuration.Configure(o =>
                    {
                        o.DefaultContextLabel = defaultContextLabel;
                        o.AddAppTag("deckhub-web");
                        o.AddServerTag(Environment.MachineName);
                        o.Enabled = true;
                        o.ReportingEnabled = true;
                    })
                    .Build();

                services.AddMetrics(metrics);
                services.AddMetricsTrackingMiddleware(o =>
                {
                    o.ApdexTSeconds = 0.1;
                    o.ApdexTrackingEnabled = true;
                });
                services.AddMetricsReportScheduler();
                //services.AddSingleton<IHostedService>(new MetricsReportWriter(metrics, TimeSpan.FromSeconds(5)));
            }

            return services;
        }
    }
}