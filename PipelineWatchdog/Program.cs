using System.Collections.Immutable;
using Azure.Monitor.OpenTelemetry.Exporter;
using Core;
using Implementation;
using Logic;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Telemetry;

namespace PipelineWatchdog;

class Program
{
    static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Configuration.AddUserSecrets<Program>();
        AddServices(builder);
        ConfigureOpenTelemetry(builder);
        
        
        // Run the application
        builder.Build().Run();
    }

    private static void AddServices(HostApplicationBuilder builder)
    {
        builder.Services.AddAzureClients(config =>
        {
            config.AddServiceBusClient(builder.Configuration.GetConnectionString("ServiceBus")).WithName("ServiceBusClient");
        });
        builder.Services.AddSingleton<IRepoDataSource, TableStorageDataSource>();
        builder.Services.AddSingleton<IRepoSource, GiteaRepoSource>();
        builder.Services.AddSingleton<IReprocessor, ServiceBusReprocessor>();
        builder.Services.AddSingleton<WatchdogMetrics>();
        builder.Services.AddHostedService<ListenerService>();
        builder.Services.AddHostedService<WatchdogService>();
    }

    private static void ConfigureOpenTelemetry(HostApplicationBuilder builder)
    {
        var applicationInsightsEnabled = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING") != null;
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource =>
            {
                resource.AddService("PipelineWatchdog");
            })
            .WithTracing(config =>
            {
                config.AddSource(nameof(WatchdogService), nameof(ListenerService), nameof(ServiceBusReprocessor),
                    nameof(TableStorageDataSource), nameof(GiteaRepoSource), nameof(ListenerLogic),
                    nameof(ReconcileLogic));
                config.AddHttpClientInstrumentation();
                config.AddOtlpExporter();
                if (applicationInsightsEnabled)
                {
                    config.AddAzureMonitorTraceExporter();
                }
            })
            .WithMetrics(config =>
            {
                config.AddMeter(WatchdogMetrics.MetricNamespace);
                config.AddOtlpExporter();
                if (applicationInsightsEnabled)
                {
                    config.AddAzureMonitorMetricExporter();
                }
            })
            ;
        builder.Logging.AddOpenTelemetry(config =>
        {
            config.IncludeScopes = true;
            config.ParseStateValues = true;
            config.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("PipelineWatchdog"));
            config.AddOtlpExporter();
            if (applicationInsightsEnabled)
            {
                config.AddAzureMonitorLogExporter();
            }
        });
    }
}