using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Options;
using ServiceBus.RequestReply.Sample.Startup.Factories;
using ServiceBus.RequestReply.Sample.Startup.Options;

namespace ServiceBus.RequestReply.Sample.Startup.Clients
{
    /// <summary>
    /// Implements a request-reply pattern, where a temporary queue is created for the response.
    /// </summary>
    public class ServiceBusRequestReplyClient : IServiceBusRequestReplyClient
    {
        private readonly ServiceBusClientFactory _clientFactory;
        private readonly ManagementClient _managementClient;
        private readonly RequestReplyClientOptions _options;

        // minimum allowed time for AutoDeleteOnIdle is 5 minutes
        private readonly long MinimumAutoDeleteOnIdleMillis = 5 * 60 * 1000;

        public ServiceBusRequestReplyClient(ServiceBusClientFactory clientFactory, ManagementClient managementClient, IOptions<RequestReplyClientOptions> options)
        {
            _clientFactory = clientFactory;
            _managementClient = managementClient;
            _options = options.Value;
        }

        public async Task<T> Request<T>(string queueName, Message message) where T : class
        {
            var temporaryQueueName = Guid.NewGuid().ToString();
            var temporaryQueueDescription = await _managementClient.CreateQueueAsync(temporaryQueueName);

            var timeoutMillis = Math.Max(MinimumAutoDeleteOnIdleMillis, 2 * _options.RequestTimeOutMillis);
            // AutoDelete in case of unexpected crash
            temporaryQueueDescription.AutoDeleteOnIdle = TimeSpan.FromMilliseconds(timeoutMillis);

            var requestClient = _clientFactory.CreateSendClient(queueName, RetryPolicy.Default);
            var receiverClient = _clientFactory.CreateReceiverClient(temporaryQueueName, ReceiveMode.ReceiveAndDelete);

            try
            {
                message.ReplyTo = temporaryQueueName;
                await requestClient.SendAsync(message);

                var reply = await receiverClient.ReceiveAsync(_options.RequestTimeout);

                return reply != null
                    ? JsonSerializer.Deserialize<T>(reply.Body)
                    : null;
            }
            finally
            {
                await _managementClient.DeleteQueueAsync(temporaryQueueName);
            }
        }
    }

    public interface IServiceBusRequestReplyClient
    {
        Task<T> Request<T>(string queueName, Message message) where T : class;
    }
}
