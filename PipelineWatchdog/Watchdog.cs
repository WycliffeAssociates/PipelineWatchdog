using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Logic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using Telemetry;

namespace PipelineWatchdog;
public class WatchdogService : BackgroundService
{
    private readonly ILogger<WatchdogService> _logger;
    private readonly TimeSpan _interval; // Set your interval here
    private readonly ReconcileLogic _logic;
    private readonly WatchdogMetrics _metrics;
    private static readonly ActivitySource _activitySource = new(nameof(WatchdogService));

    public WatchdogService(ILogger<WatchdogService> logger, IConfiguration configuration, IReprocessor reprocessor, IRepoDataSource cache, IRepoSource source, WatchdogMetrics metrics)
    {
        _logger = logger;
        // Read the interval from configuration or set a default value
        var delaySeconds = configuration.GetValue("IntervalInMinutes", 10);
        _interval = TimeSpan.FromMinutes(delaySeconds);
        _logic = new ReconcileLogic(source,cache, reprocessor, logger, metrics);
        _metrics = metrics;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Start span here because we need it to actually complete
            using var activity = _activitySource.StartActivity();
            _logger.LogInformation("Watchdog task running at: {Time}", DateTimeOffset.Now);
            await _logic.RunAsync();
            await Task.Delay(_interval, stoppingToken);
        }
    }
}
