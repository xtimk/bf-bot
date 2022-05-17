namespace bf_bot.Extensions
{
    public static class HttpClientExtension
    {
        public static void AddBaseHeaders(this HttpRequestMessage request, string appKey)
        {
            request.Headers.Add("X-Application", appKey);
            request.Headers.Add("Accept", "application/json");
        }

        public static void AddAuthHeader(this HttpRequestMessage request, string authHeader)
        {
            request.Headers.Add("X-Authentication", authHeader);
        }
        // function to add to headers the auth token here..
    }
}