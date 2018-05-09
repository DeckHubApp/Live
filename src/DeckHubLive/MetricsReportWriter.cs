using System;
using System.Threading;
using System.Threading.Tasks;
using App.Metrics;
using Microsoft.Extensions.Hosting;

namespace DeckHubLive
{
    public class MetricsReportWriter : IHostedService
    {
        private readonly IMetricsRoot _metrics;
        private readonly TimeSpan _interval;
        private bool _run;
        public MetricsReportWriter(IMetricsRoot metrics, TimeSpan interval)
        {
            _metrics = metrics;
            _interval = interval;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _run = true;
            while (_run && !cancellationToken.IsCancellationRequested)
            {
                await Task.WhenAll(_metrics.ReportRunner.RunAllAsync(cancellationToken));
                await Task.Delay(_interval, cancellationToken);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _run = false;
            await Task.Delay(_interval, cancellationToken);
        }
    }
}