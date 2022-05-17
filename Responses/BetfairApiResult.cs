namespace bf_bot
{
    public class BetfairApiResult
    {
        public bool IsOk { get; set; }
        // public T? SuccessDetails { get; set; }
        public HttpResponseMessage HttpResponseMessage { get; set; }

        public Exception Exception { get; set; }

        public BetfairApiResult()
        {
            IsOk = false;
            HttpResponseMessage = null;
            Exception = null;
        }
    }
}