using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using ServiceBus.RequestReply.Sample.Startup.Dtos;
using ServiceBus.RequestReply.Sample.Startup.Factories;
using Microsoft.Azure.Functions.Worker;

namespace ServiceBus.RequestReply.Sample.Startup.Functions
{
    public class QueueConsumer
    {
        private readonly ServiceBusClientFactory _clientFactory;
        private readonly ILogger<QueueConsumer> _logger;

        public QueueConsumer(ServiceBusClientFactory clientFactory, ILogger<QueueConsumer> logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        [Function("QueueProcessor")]
        public async Task ConsumeFromQueue(
            [ServiceBusTrigger("%QueueName%", Connection = EnvironmentVariableNames.ServiceBusConnectionString)]
            ServiceBusReceivedMessage message)
        {
            var request = JsonSerializer.Deserialize<Request>(message.Body);
            if (request == null)
            {
                // ignore empty message
                return;
            }

            _logger.LogInformation("Read message from queue: {request}", request);

            var replyQueueName = message.ReplyTo;
            if (string.IsNullOrWhiteSpace(replyQueueName))
            {
                return;
            }

            _logger.LogInformation("Reply requested to queue {replyQueueName}", replyQueueName);
            var acknowledgement = new Reply
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Name = request.Name
            };

            var responseBytes = JsonSerializer.SerializeToUtf8Bytes(acknowledgement, Constants.DefaultJsonSerializerOptions);
            var responseMessage = new ServiceBusMessage(responseBytes)
            {
                ReplyToSessionId = message.SessionId
            };

            await using var queueClient = _clientFactory.CreateSendClient(replyQueueName);
            await queueClient.SendMessageAsync(responseMessage);
        }
    }
}
