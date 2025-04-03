using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Logic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PipelineWatchdog;
public class WatchdogService : BackgroundService
{
    private readonly ILogger<WatchdogService> _logger;
    private readonly TimeSpan _interval; // Set your interval here
    private readonly ReconcileLogic _logic;

    public WatchdogService(ILogger<WatchdogService> logger, IConfiguration configuration, IReprocessor reprocessor, IRepoDataSource cache, IRepoSource source)
    {
        _logger = logger;
        // Read the interval from configuration or set a default value
        var delaySeconds = configuration.GetValue("IntervalInMinutes", 10);
        _interval = TimeSpan.FromMinutes(delaySeconds);
        _logic = new ReconcileLogic(source,cache, reprocessor, logger);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Watchdog task running at: {Time}", DateTimeOffset.Now);
            await _logic.RunAsync();
            await Task.Delay(_interval, stoppingToken);
        }
    }
}
