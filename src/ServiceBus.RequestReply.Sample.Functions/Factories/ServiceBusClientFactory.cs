using System;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Options;
using ServiceBus.RequestReply.Sample.Startup.Options;

namespace ServiceBus.RequestReply.Sample.Startup.Factories
{
    /// <summary>
    /// Creates Queue/MessageReceiver clients, reusing the same connection.
    /// </summary>
    public class ServiceBusClientFactory
    {
        private readonly ServiceBusConnection _connection;

        public ServiceBusClientFactory(IOptions<ServiceBusConnectionOptions> options)
        {
            _connection = new ServiceBusConnection(options.Value.ConnectionString);
        }

        public virtual IQueueClient CreateSendClient(string queueName, RetryPolicy retryPolicy = default)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentNullException(queueName);
            }

            return new QueueClient(_connection, queueName, default, retryPolicy);
        }

        public virtual IMessageReceiver CreateReceiverClient(string queueName, ReceiveMode receiveMode = default)
        {
            if (string.IsNullOrWhiteSpace(queueName))
            {
                throw new ArgumentNullException(queueName);
            }

            return new MessageReceiver(_connection, queueName, receiveMode);
        }
    }
}
