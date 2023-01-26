using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ServiceBus.RequestReply.Sample.Startup.Options;

namespace ServiceBus.RequestReply.Sample.Startup.Factories
{
    /// <summary>
    /// Creates Queue/MessageReceiver clients, reusing the same connection.
    /// </summary>
    public sealed class ServiceBusClientFactory : IAsyncDisposable
    {
        private readonly ServiceBusClient _client;

        public ServiceBusClientFactory(IOptions<ServiceBusConnectionOptions> options)
        {
            _client = new ServiceBusClient(options.Value.ConnectionString);
        }

        /// <summary>
        /// Creates a client for sending messages to a queue.
        /// Caller should dispose the client after use
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ServiceBusSender CreateSendClient(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentNullException(queueName);
            }

            return _client.CreateSender(queueName);
        }

        /// <summary>
        /// Creates a client for receiving messages from a queue.
        /// Caller should dispose the client after use
        /// </summary>
        /// <param name="queueName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public ServiceBusReceiver CreateReceiverClient(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentNullException(queueName);
            }

            return _client.CreateReceiver(queueName);
        }

        public ValueTask DisposeAsync()
        {
            return _client.DisposeAsync();
        }
    }
}
