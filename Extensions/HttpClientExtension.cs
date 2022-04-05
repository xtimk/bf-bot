using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace bf_bot.Extensions
{
    public static class HttpClientExtension
    {
        public static void AddBaseHeaders(this HttpRequestMessage request)
        {
            AppSettings settings = AppSettings.Instance;
            request.Headers.Add("X-Application", settings.AppKey);
            request.Headers.Add("Accept", "application/json");
        }
        // function to add to headers the auth token here..
    }
}