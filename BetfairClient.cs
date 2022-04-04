using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using bf_bot.Extentions;

namespace bf_bot
{
    public class BetfairClient
    {
        static readonly HttpClient client = new HttpClient();
        public string? AuthToken { get; set; }
        public BetfairClient()
        {
            // nothing todo here..
        }
        public async Task<BetfairApiResult<BetfairLoginResponse>> Login()
        {
            BetfairApiResult<BetfairLoginResponse> result = new BetfairApiResult<BetfairLoginResponse>
            {
                IsSuccessfull = false,
                Details = null
            };

            AppSettings settings = AppSettings.Instance;
            // client.BaseAddress = new Uri(settings.AuthEndPoint);
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", settings.Username),
                new KeyValuePair<string, string>("password", settings.Password)
            });

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, settings.AuthEndPoint);
            requestMessage.AddBaseHeaders();
            requestMessage.Content = content;

            HttpResponseMessage httpResponse = await client.SendAsync(requestMessage);
            httpResponse.EnsureSuccessStatusCode();
            
            if(httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                try
                {
                    var response = JsonSerializer.Deserialize<BetfairLoginResponse>(httpResponseBody);
                    AuthToken = response?.Token;
                    Console.WriteLine(AuthToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

                if (String.IsNullOrEmpty(AuthToken))
                {
                    result.IsSuccessfull = false;
                    result.Details = null;
                }
                else
                {
                    result.IsSuccessfull = true;
                    result.Details = null;
                }
            }


            return result;

        }
    }
}