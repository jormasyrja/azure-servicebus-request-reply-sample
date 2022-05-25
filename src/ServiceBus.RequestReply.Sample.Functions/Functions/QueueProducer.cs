using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
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
            Request request)
        {
            _logger.LogInformation($"Received request from API to create: {request}");

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var response = await _requestReplyClient.Request<Reply>(_options.TargetQueueName, request);
                stopwatch.Stop();

                _logger.LogInformation($"Received response: {response}, took {stopwatch.ElapsedMilliseconds} ms");

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
