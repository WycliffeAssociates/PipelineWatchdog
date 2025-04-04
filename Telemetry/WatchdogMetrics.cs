using System.Diagnostics.Metrics;

namespace Telemetry;

public class WatchdogMetrics
{
    public const string MetricNamespace = "PipelineWatchdog";
    private readonly Meter _meter;
    private readonly Counter<long> _messagesSeen;
    private readonly Counter<long> _createsSent;
    private readonly Counter<long> _deletesSent;
    private readonly Counter<long> _wacsRequestsSent;

    public WatchdogMetrics(IMeterFactory meterFactory)
    {
        _meter = meterFactory.Create(MetricNamespace);
        _messagesSeen = _meter.CreateCounter<long>("messages_seen", "count", "The number of messages seen");
        _createsSent = _meter.CreateCounter<long>("creates_sent", "count", "The number of creates sent");
        _deletesSent = _meter.CreateCounter<long>("deletes_sent", "count", "The number of deletes sent");
        _wacsRequestsSent = _meter.CreateCounter<long>("wacs_requests_sent", "count", "The number of requests sent to WACS");
    }

    public void IncrementMessagesSeen(long count)
    {
        _messagesSeen.Add(count);
    }
    public void CreatesSent(long count)
    {
        _createsSent.Add(count);
    }
    public void DeletesSent(long count)
    {
        _deletesSent.Add(count);
    }
    public void WacsRequestsSent(long count)
    {
        _wacsRequestsSent.Add(count);
    }
}