using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
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
        private readonly ServiceBusAdministrationClient _administrationClient;
        private readonly RequestReplyClientOptions _options;

        // minimum allowed time for AutoDeleteOnIdle is 5 minutes
        private const long MinimumAutoDeleteOnIdleMillis = 5 * 60 * 1000;

        public ServiceBusRequestReplyClient(ServiceBusClientFactory clientFactory, ServiceBusAdministrationClient administrationClient, IOptions<RequestReplyClientOptions> options)
        {
            _clientFactory = clientFactory;
            _administrationClient = administrationClient;
            _options = options.Value;
        }

        public async Task<T> Request<T>(string queueName, object payload) where T : class
        {
            var temporaryQueueName = Guid.NewGuid().ToString();
            
            var timeoutMillis = Math.Max(MinimumAutoDeleteOnIdleMillis, 2 * _options.RequestTimeOutMillis);
            var autoDeleteOnIdleTimespan = TimeSpan.FromMilliseconds(timeoutMillis);

            var createQueueOptions = new CreateQueueOptions(temporaryQueueName)
            {
                AutoDeleteOnIdle = autoDeleteOnIdleTimespan,
                Name = temporaryQueueName
            };

            await _administrationClient.CreateQueueAsync(createQueueOptions);

            var sender = _clientFactory.CreateSendClient(queueName);
            var receiver = _clientFactory.CreateReceiverClient(temporaryQueueName);

            try
            {
                var outboundMessage = new ServiceBusMessage(JsonSerializer.SerializeToUtf8Bytes(payload, Constants.DefaultJsonSerializerOptions))
                {
                    ReplyTo = temporaryQueueName
                };
                await sender.SendMessageAsync(outboundMessage);

                var reply = await receiver.ReceiveMessageAsync(_options.RequestTimeout);

                return reply != null
                    ? JsonSerializer.Deserialize<T>(reply.Body, Constants.DefaultJsonSerializerOptions)
                    : null;
            }
            finally
            {
                await _administrationClient.DeleteQueueAsync(temporaryQueueName)
                    .ConfigureAwait(false);
            }
        }
    }

    public interface IServiceBusRequestReplyClient
    {
        Task<T> Request<T>(string queueName, object payload) where T : class;
    }
}
