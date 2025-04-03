using Core;
using Implementation;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace PipelineWatchdog;

class Program
{
    static void Main(string[] args)
    {
        var host = Host.CreateApplicationBuilder(args);
        host.Configuration.AddUserSecrets<Program>();
        host.Services.AddAzureClients(builder =>
        {
            builder.AddServiceBusClient(host.Configuration.GetConnectionString("ServiceBus")).WithName("ServiceBusClient");
        });
        host.Services.AddSingleton<IRepoDataSource, TableStorageDataSource>();
        host.Services.AddSingleton<IRepoSource, GiteaRepoSource>();
        host.Services.AddSingleton<IReprocessor, ServiceBusReprocessor>();
        host.Services.AddHostedService<ListenerService>();
        host.Services.AddHostedService<WatchdogService>();
        
        // Run the application
        host.Build().Run();
    }
}