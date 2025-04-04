using System.Diagnostics;
using System.Text.Json;
using Azure.Core.Extensions;
using Azure.Messaging.ServiceBus;
using Core;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Implementation;

public class ServiceBusReprocessor : IReprocessor, IAsyncDisposable
{
    private readonly ILogger<ServiceBusReprocessor> _logger;
    private readonly ServiceBusSender _sender;
    private static readonly ActivitySource _activitySource = new(nameof(ServiceBusReprocessor));
    public ServiceBusReprocessor(IConfiguration configuration, ILogger<ServiceBusReprocessor> logging, IAzureClientFactory<ServiceBusClient> factory)
    {
        var serviceBusClient = factory.CreateClient("ServiceBusClient");
        _sender = serviceBusClient.CreateSender("WACSEvent");
        _logger = logging;
    }
    public async Task SendCreate(Repo repo)
    {
        using var activity = _activitySource.StartActivity();
        _logger.LogDebug("Sending Create");
        await _sender.SendMessageAsync(CreateMessage(repo.ToWACSMessage("repo", "created")));
    }

    public async Task SendDelete(Repo repo)
    {
        using var activity = _activitySource.StartActivity();
        _logger.LogDebug("Sending Delete");
        await _sender.SendMessageAsync(CreateMessage(repo.ToWACSMessage("repo", "deleted")));
    }
    private ServiceBusMessage CreateMessage(WACSMessage input)
    {
        var message = new ServiceBusMessage(JsonSerializer.Serialize(input));
        message.ApplicationProperties["EventType"] = input.EventType;
        message.ApplicationProperties["Action"] = input.Action;
        return message;
    }

    public async ValueTask DisposeAsync()
    {
        await _sender.DisposeAsync();
    }
}