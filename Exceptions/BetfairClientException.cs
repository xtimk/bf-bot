using Microsoft.Extensions.Logging;

namespace bf_bot.Exceptions
{
    public class BetfairClientException : Exception
    {
        public BetfairClientException()
        {
        }
        public string BFMessage { get; set; }
        public string Body { get; set; }
        public Exception Inner { get; set; }

        public BetfairClientException(ILogger logger, string message) : base(message)
        {
            BFMessage = message;
            logger.LogError("Exception encountered: " + message);
        }
        public BetfairClientException(ILogger logger, string message, string body) : base(message)
        {
            Body = body;
            BFMessage = message;
            logger.LogWarning(message + ". Details: " + body);
        }
        public BetfairClientException(ILogger logger, string message, Exception inner) : base(message, inner)
        {
            Inner = inner;
            logger.LogError("Exception encountered: " + message + ". Inner exception: " + inner);
        }
    }
}