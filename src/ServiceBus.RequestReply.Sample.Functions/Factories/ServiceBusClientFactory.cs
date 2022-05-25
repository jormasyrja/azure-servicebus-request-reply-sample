using System;
using System.Collections.Concurrent;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using ServiceBus.RequestReply.Sample.Startup.Options;

namespace ServiceBus.RequestReply.Sample.Startup.Factories
{
    /// <summary>
    /// Creates Queue/MessageReceiver clients, reusing the same connection.
    /// </summary>
    public sealed class ServiceBusClientFactory
    {
        private readonly ServiceBusClient _client;
        private readonly ConcurrentDictionary<string, ServiceBusSender> _senderCache;
        private readonly ConcurrentDictionary<string, ServiceBusReceiver> _receiverCache;

        public ServiceBusClientFactory(IOptions<ServiceBusConnectionOptions> options)
        {
            _client = new ServiceBusClient(options.Value.ConnectionString);

            _senderCache = new ConcurrentDictionary<string, ServiceBusSender>();
            _receiverCache = new ConcurrentDictionary<string, ServiceBusReceiver>();
        }

        public ServiceBusSender CreateSendClient(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentNullException(queueName);
            }

            return _senderCache.GetOrAdd(queueName, qName => _client.CreateSender(qName));
        }

        public ServiceBusReceiver CreateReceiverClient(string queueName)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentNullException(queueName);
            }

            return _receiverCache.GetOrAdd(queueName, qName => _client.CreateReceiver(qName));
        }
    }
}
