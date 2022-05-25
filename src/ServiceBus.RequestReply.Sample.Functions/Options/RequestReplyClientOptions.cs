using System;

namespace ServiceBus.RequestReply.Sample.Startup.Options
{
    public class RequestReplyClientOptions
    { 
        public long RequestTimeOutMillis { get; set; } = Constants.DefaultRequestTimeoutMillis;
        public TimeSpan RequestTimeout => TimeSpan.FromMilliseconds(RequestTimeOutMillis);
    }
}
