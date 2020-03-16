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

        public async Task<T> Request<T>(string queueName, object payload) where T : class
        {
            var temporaryQueueName = Guid.NewGuid().ToString();
            
            var timeoutMillis = Math.Max(MinimumAutoDeleteOnIdleMillis, 2 * _options.RequestTimeOutMillis);
            var autoDeleteOnIdleTimespan = TimeSpan.FromMilliseconds(timeoutMillis);

            var temporaryQueueDescription = new QueueDescription(temporaryQueueName)
            {
                AutoDeleteOnIdle = autoDeleteOnIdleTimespan
            };
            await _managementClient.CreateQueueAsync(temporaryQueueDescription);

            var requestClient = _clientFactory.CreateSendClient(queueName, RetryPolicy.Default);
            var receiverClient = _clientFactory.CreateReceiverClient(temporaryQueueName, ReceiveMode.ReceiveAndDelete);

            try
            {
                var outboundMessage = new Message(JsonSerializer.SerializeToUtf8Bytes(payload))
                {
                    ReplyTo = temporaryQueueName
                };
                await requestClient.SendAsync(outboundMessage);

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
        Task<T> Request<T>(string queueName, object payload) where T : class;
    }
}
