using System;

namespace ServiceBus.RequestReply.Sample.Startup.Dtos
{
    public class CreateWorkQueueItemRequest
    {
        public string Name { get; set; }
        public int Amount { get; set; }
        public DateTime DueDate { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}, Amount: {Amount}, DueDate: {DueDate:MM-dd-yyyy}";
        }
    }
}
