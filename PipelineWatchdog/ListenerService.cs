using System.Text.Json;
using Azure.Core.Extensions;
using Azure.Messaging.ServiceBus;
using Core;
using Logic;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace PipelineWatchdog;

public class ListenerService: IHostedService, IAsyncDisposable
{
    private readonly ILogger<ListenerService> _logger;
    private readonly ServiceBusProcessor _processor;
    private readonly ListenerLogic _logic;
    private const string TopicName = "WACSEvent";
    private const string SubscriptionName = "Watchdog";
    public ListenerService(ILogger<ListenerService> logger, IRepoDataSource dataSource, IAzureClientFactory<ServiceBusClient> azureClientFactory)
    {
        _logger = logger;
        var client = azureClientFactory.CreateClient("ServiceBusClient");
        _processor = client.CreateProcessor(TopicName, SubscriptionName);
        _processor.ProcessMessageAsync += ProcessMessage;
        _processor.ProcessErrorAsync += ProcessError;
        _logic = new ListenerLogic(dataSource, logger);
    }

    private Task ProcessError(ProcessErrorEventArgs arg)
    {
        _logger.LogError(arg.Exception, "Error processing message");
        return Task.CompletedTask;
    }

    private async Task ProcessMessage(ProcessMessageEventArgs arg)
    {
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
        await _processor.StartProcessingAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _processor.DisposeAsync();
    }
}