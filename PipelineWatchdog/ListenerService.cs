using System.Diagnostics;
using System.Text.Json;
using Azure.Core.Extensions;
using Azure.Messaging.ServiceBus;
using Core;
using Logic;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telemetry;

namespace PipelineWatchdog;

public class ListenerService: IHostedService, IAsyncDisposable
{
    private readonly ILogger<ListenerService> _logger;
    private readonly ServiceBusProcessor _processor;
    private readonly ListenerLogic _logic;
    private const string TopicName = "WACSEvent";
    private const string SubscriptionName = "Watchdog";
    private static readonly ActivitySource _activitySource = new(nameof(ListenerService));
    private readonly WatchdogMetrics _metrics;
    public ListenerService(ILogger<ListenerService> logger, IRepoDataSource dataSource, IAzureClientFactory<ServiceBusClient> azureClientFactory, WatchdogMetrics metrics)
    {
        _logger = logger;
        var client = azureClientFactory.CreateClient("ServiceBusClient");
        _processor = client.CreateProcessor(TopicName, SubscriptionName);
        _processor.ProcessMessageAsync += ProcessMessage;
        _processor.ProcessErrorAsync += ProcessError;
        _logic = new ListenerLogic(dataSource, logger);
        _metrics = metrics;
    }

    private Task ProcessError(ProcessErrorEventArgs arg)
    {
        using var activity = _activitySource.StartActivity();
        _logger.LogError(arg.Exception, "Error processing message");
        return Task.CompletedTask;
    }

    private async Task ProcessMessage(ProcessMessageEventArgs arg)
    {
        using var activity = _activitySource.StartActivity();
        _metrics.IncrementMessagesSeen(1);
        var messageBody = arg.Message.Body.ToString();
        var parsedMessage = JsonSerializer.Deserialize<WACSMessage>(messageBody);
        if (parsedMessage == null)
        {
            _logger.LogError("Failed to parse message: {MessageBody}", messageBody);
            await arg.CompleteMessageAsync(arg.Message);
            return;
        }
        await _logic.Handle(parsedMessage);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting listener");
        await _processor.StartProcessingAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping listener");
        await _processor.StopProcessingAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _processor.DisposeAsync();
    }
}