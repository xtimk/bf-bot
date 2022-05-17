using Microsoft.Extensions.Logging;

namespace bf_bot.Exceptions
{
    public class BetfairClientException : Exception
    {
        public BetfairClientException()
        {
        }

        public BetfairClientException(ILogger logger, string message) : base(message)
        {
            logger.LogError("Exception encountered: " + message);
        }

        public BetfairClientException(ILogger logger, string message, Exception inner) : base(message, inner)
        {
            logger.LogError("Exception encountered: " + message + ". Inner exception: " + inner);
        }
    }
}