using System;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceBus.RequestReply.Sample.Startup;
using ServiceBus.RequestReply.Sample.Startup.Clients;
using ServiceBus.RequestReply.Sample.Startup.Factories;
using ServiceBus.RequestReply.Sample.Startup.Options;

[assembly: FunctionsStartup(typeof(Startup))]
namespace ServiceBus.RequestReply.Sample.Startup
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", true)
                .AddEnvironmentVariables()
                .Build();

            var serviceBusConnectionString =
                configuration.GetValue<string>(EnvironmentVariableNames.ServiceBusConnectionString);

            builder.Services.Configure<ServiceBusConnectionOptions>(options =>
            {
                options.ConnectionString = serviceBusConnectionString;
            });

            builder.Services.Configure<RequestReplyClientOptions>(options =>
            {
                options.RequestTimeOutMillis = configuration.GetValue(EnvironmentVariableNames.RequestTimeoutMillis, Constants.DefaultRequestTimeoutMillis);
            });

            builder.Services.Configure<QueueProducerOptions>(options =>
            {
                options.TargetQueueName = configuration.GetValue<string>(EnvironmentVariableNames.QueueName);
            });

            builder.Services.AddSingleton<ServiceBusClientFactory>();
            builder.Services.AddScoped<IServiceBusRequestReplyClient, ServiceBusRequestReplyClient>();
            builder.Services.AddSingleton(provider => new ManagementClient(serviceBusConnectionString));
        }
    }
}
