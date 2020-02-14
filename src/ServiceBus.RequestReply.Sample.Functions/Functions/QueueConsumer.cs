using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using ServiceBus.RequestReply.Sample.Startup.Dtos;
using ServiceBus.RequestReply.Sample.Startup.Factories;

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

        [FunctionName("QueueProcessor")]
        public async Task ConsumeFromQueue(
            [ServiceBusTrigger("%QueueName%", Connection = EnvironmentVariableNames.ServiceBusConnectionString)]
            Message message)
        {
            var messageJson = Encoding.UTF8.GetString(message.Body);
            var request = JsonSerializer.Deserialize<Request>(messageJson);

            _logger.LogInformation($"Read message from queue: {request}");

            var replyQueueName = message.ReplyTo;
            if (string.IsNullOrWhiteSpace(replyQueueName))
            {
                return;
            }

            _logger.LogInformation($"Reply requested to queue {replyQueueName}");
            var acknowledgement = new Reply
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.UtcNow,
                Name = request.Name
            };

            var responseBytes = JsonSerializer.SerializeToUtf8Bytes(acknowledgement);
            var responseMessage = new Message(responseBytes)
            {
                ReplyToSessionId = message.SessionId
            };

            var queueClient = _clientFactory.CreateSendClient(replyQueueName);
            await queueClient.SendAsync(responseMessage);
        }
    }
}
