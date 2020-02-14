namespace ServiceBus.RequestReply.Sample.Startup.Dtos
{
    public class Request
    {
        public string Name { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}";
        }
    }
}
