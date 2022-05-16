using System.Text;
using System.Text.Json;
using bf_bot.Extensions;

namespace bf_bot
{
    public class BetfairClient
    {
        public string AuthToken { get; set; }
        protected BetfairClientInitializer _betfairSettings;
        public BetfairClient(BetfairClientInitializer _betfairSettings)
        {
            if(Utility.AreAllPropNotNull(_betfairSettings))
            {
                this._betfairSettings = _betfairSettings;
            }
            else
            {
                throw new ArgumentNullException("Cannot read some of the props setted in the appsetting.json file.");
            }
        }

        public async Task<T> Invoke<T>(string method, IDictionary<string, object> args) where T : BetfairApiResult, new()
        {
            // init result
            T result = new T();

            if (method == null)
                throw new ArgumentNullException("method");
            if (method.Length == 0)
                throw new ArgumentException(null, "method");

            var restEndpoint = _betfairSettings?.BetfairEndpoints?.BettingEndpoint + method + "/";

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, _betfairSettings?.BetfairEndpoints?.AuthEndpoint);

            var appKey = _betfairSettings?.BetfairLoginCredentials?.AppKey;
            if (appKey == null)
                throw new Exception("AppKey should not be null here. This exception should never be raised.");
            
            requestMessage.AddBaseHeaders(appKey);

            // if there is an auth token I add it, otherwhise no.
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

                    var jres = JsonSerializer.Deserialize<T>(httpResponseBody);
                    if (jres == null)
                        throw new Exception("Error while deserializing object.");
                    result = jres;
                       
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

            var bf_username = _betfairSettings?.BetfairLoginCredentials?.Username;
            var bf_password = _betfairSettings?.BetfairLoginCredentials?.Password;

            if (bf_username == null || bf_password == null)
                throw new Exception("Cannot read username or password from config file. This exception should never happen.");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", bf_username),
                new KeyValuePair<string, string>("password", bf_password)
            });

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, _betfairSettings?.BetfairEndpoints?.AuthEndpoint);

            var appKey = _betfairSettings?.BetfairLoginCredentials?.AppKey;
            if (appKey == null)
                throw new Exception("AppKey should not be null here. This exception should never be raised.");

            requestMessage.AddBaseHeaders(appKey);
            requestMessage.Content = content;

            HttpResponseMessage httpResponse = await HttpClientSingleton.Instance.Client.SendAsync(requestMessage);
            
            if(httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                try
                {
                    var jres = JsonSerializer.Deserialize<BetfairLoginResponse>(httpResponseBody);
                    if (jres == null)
                        throw new Exception("Error while deserializing object.");
                    result = jres;

                    if(result.Status == "SUCCESS")
                    {
                        AuthToken = result.Token;
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