using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceBus.RequestReply.Sample.Startup.Clients;
using ServiceBus.RequestReply.Sample.Startup.Dtos;
using ServiceBus.RequestReply.Sample.Startup.Options;

namespace ServiceBus.RequestReply.Sample.Startup.Functions
{
    public class QueueProducer
    {
        private readonly IServiceBusRequestReplyClient _requestReplyClient;
        private readonly QueueProducerOptions _options;
        private readonly ILogger<QueueProducer> _logger;

        public QueueProducer(IServiceBusRequestReplyClient requestReplyClient, IOptions<QueueProducerOptions> options, ILogger<QueueProducer> logger)
        {
            _logger = logger;
            _options = options.Value;
            _requestReplyClient = requestReplyClient;
        }

        [FunctionName("QueueProducer")]
        public async Task<IActionResult> PushMessageToQueue(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "messages")]
            CreateWorkQueueItemRequest request)
        {
            _logger.LogInformation($"Received request from API to create: {request}");

            try
            {
                var outboundMessage = new Message(JsonSerializer.SerializeToUtf8Bytes(request));
                var response = await _requestReplyClient.Request<WorkQueueItemAcknowledgement>(_options.TargetQueueName, outboundMessage);

                _logger.LogInformation($"Received response: {response}");

                return new OkObjectResult(response);
            }
            catch (ServiceBusException sbe)
            {
                _logger.LogError(sbe, "Could not send request to ServiceBus");
                return new StatusCodeResult((int)HttpStatusCode.ServiceUnavailable);
            }
        }
    }
}
