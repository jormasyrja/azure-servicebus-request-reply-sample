using System;

namespace ServiceBus.RequestReply.Sample.Startup.Dtos
{
    public class Reply
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return $"Id: {Id}, Timestamp: {Timestamp}, Name: {Name}";
        }
    }
}
