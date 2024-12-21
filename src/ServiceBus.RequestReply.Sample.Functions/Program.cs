using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ServiceBus.RequestReply.Sample.Startup;
using ServiceBus.RequestReply.Sample.Startup.Clients;
using ServiceBus.RequestReply.Sample.Startup.Factories;
using ServiceBus.RequestReply.Sample.Startup.Options;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(((context, services) =>
    {
        var serviceBusConnectionString = context.Configuration.GetValue<string>(EnvironmentVariableNames.ServiceBusConnectionString);

        services.Configure<ServiceBusConnectionOptions>(options =>
        {
            options.ConnectionString = serviceBusConnectionString;
        });

        services.Configure<RequestReplyClientOptions>(options =>
        {
            options.RequestTimeOutMillis = context.Configuration.GetValue(EnvironmentVariableNames.RequestTimeoutMillis, Constants.DefaultRequestTimeoutMillis);
        });

        services.Configure<QueueProducerOptions>(options =>
        {
            options.TargetQueueName = context.Configuration.GetValue<string>(EnvironmentVariableNames.QueueName);
        });

        services.AddSingleton<ServiceBusClientFactory>();
        services.AddScoped<IServiceBusRequestReplyClient, ServiceBusRequestReplyClient>();
        services.AddSingleton(_ => new ServiceBusAdministrationClient(serviceBusConnectionString));
    }))
    .Build();

host.Run();
