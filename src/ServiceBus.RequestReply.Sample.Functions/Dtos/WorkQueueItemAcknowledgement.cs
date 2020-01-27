using System;

namespace ServiceBus.RequestReply.Sample.Startup.Dtos
{
    public class WorkQueueItemAcknowledgement
    {
        public Guid WorkId { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"WorkId: {WorkId}, Timestamp: {Timestamp}";
        }
    }
}
