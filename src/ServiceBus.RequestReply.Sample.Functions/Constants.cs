namespace ServiceBus.RequestReply.Sample.Startup
{
    public static class EnvironmentVariableNames
    {
        public const string QueueName = "QueueName";
        public const string ServiceBusConnectionString = "ServiceBusConnectionString";
        public const string RequestTimeoutMillis = "RequestTimeoutInMilliseconds";
    }

    public static class Constants
    {
        public const long DefaultRequestTimeoutMillis = 2 * 60 * 1000;
    }
}
