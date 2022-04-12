using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using bf_bot.Extensions;

namespace bf_bot
{
    public class BetfairClient
    {
        public string? AuthToken { get; set; }
        protected BetfairClientInitializer _betfairSettings;
        public BetfairClient(BetfairClientInitializer _betfairSettings)
        {
            if(Utility.AreAllPropNotNull(_betfairSettings))
            {
                this._betfairSettings = _betfairSettings;
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        public async Task<T> Invoke<T>(string method, IDictionary<string, object> args = null) where T : BetfairApiResult, new()
        {
            // init result
            T result = new T();

            if (method == null)
                throw new ArgumentNullException("method");
            if (method.Length == 0)
                throw new ArgumentException(null, "method");

            var restEndpoint = _betfairSettings?.BetfairEndpoints?.BettingEndpoint + method + "/";

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, _betfairSettings?.BetfairEndpoints?.AuthEndpoint);
            requestMessage.AddBaseHeaders(_betfairSettings.BetfairLoginCredentials.AppKey);
            
            if(AuthToken != null)
                requestMessage.AddAuthHeader(AuthToken);

            var postData = new StringContent(JsonSerializer.Serialize<IDictionary<string, object>>(args) + "}", Encoding.UTF8, "application/json");
            requestMessage.Content = postData;

            Console.WriteLine("\nCalling: " + method + " With args: " + postData);
            HttpResponseMessage httpResponse = await HttpClientSingleton.Instance.Client.SendAsync(requestMessage);
            if(httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    string httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                    if(httpResponseBody == null)
                        throw new HttpRequestException();
                    result = JsonSerializer.Deserialize<T>(httpResponseBody);
                    result.IsOk = true;
                    result.HttpResponseMessage = httpResponse;
                    return result;
                }
                catch (Exception e)
                {
                    result.IsOk = false;
                    result.Exception = e;
                    result.HttpResponseMessage = httpResponse;
                    return result;
                }

            }
            else
            {
                string errorMessage = "Exception when calling method <" + method + ">. Response code <" + httpResponse.StatusCode + "> is not OK.";
                throw new HttpRequestException(errorMessage);
            }
        }


        public async Task<BetfairLoginResponse> Login()
        {
            BetfairLoginResponse result = new BetfairLoginResponse
            {
                IsOk = false
            };

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", _betfairSettings?.BetfairLoginCredentials?.Username),
                new KeyValuePair<string, string>("password", _betfairSettings?.BetfairLoginCredentials?.Password)
            });

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, _betfairSettings?.BetfairEndpoints?.AuthEndpoint);
            requestMessage.AddBaseHeaders(_betfairSettings.BetfairLoginCredentials.AppKey);
            requestMessage.Content = content;

            HttpResponseMessage httpResponse = await HttpClientSingleton.Instance.Client.SendAsync(requestMessage);
            
            if(httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                try
                {
                    result = JsonSerializer.Deserialize<BetfairLoginResponse>(httpResponseBody);

                    if(result.Status == "SUCCESS")
                    {
                        AuthToken = result?.Token;
                        result.HttpResponseMessage = httpResponse;
                        result.IsOk = true;
                    }
                    else
                    {
                        result.IsOk = false;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else
            {
                result.IsOk = false;
                result.HttpResponseMessage = httpResponse;
            }
            return result;
        }
    }
}