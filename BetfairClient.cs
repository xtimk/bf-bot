using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using bf_bot.Extentions;

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

        // public T Invoke<T>(string method, IDictionary<string, object> args = null)
        // {
        //     if (method == null)
        //         throw new ArgumentNullException("method");
        //     if (method.Length == 0)
        //         throw new ArgumentException(null, "method");

        //     var restEndpoint = EndPoint + method + "/";
        //     var request = CreateWebRequest(restEndpoint);

        //     var postData = JsonSerializer.Serialize<IDictionary<string, object>>(args) + "}";

        //     Console.WriteLine("\nCalling: " + method + " With args: " + postData);

        //     var bytes = Encoding.GetEncoding("UTF-8").GetBytes(postData);
        //     request.ContentLength = bytes.Length;

        //     using (Stream stream = request.GetRequestStream())
        //     {
        //         stream.Write(bytes, 0, bytes.Length);
        //     }

        //     using (HttpWebResponse response = (HttpWebResponse)GetWebResponse(request))
           
        //     using (Stream stream = response.GetResponseStream())
        //     using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
        //     {

        //         var jsonResponse = reader.ReadToEnd();
        //         Console.WriteLine("\nGot response: " + jsonResponse);
                
        //         if (response.StatusCode != HttpStatusCode.OK) {
        //             throw ReconstituteException(JsonConvert.Deserialize<Api_ng_sample_code.TO.Exception>(jsonResponse));
        //         }
        //         return JsonSerializer.Deserialize<T>(jsonResponse);

        //     }
        // }


        public async Task<BetfairApiResult<BetfairLoginResponse>> Login()
        {
            BetfairApiResult<BetfairLoginResponse> result = new BetfairApiResult<BetfairLoginResponse>
            {
                IsSuccessfull = false,
                Details = null
            };

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", _betfairSettings?.BetfairLoginCredentials?.Username),
                new KeyValuePair<string, string>("password", _betfairSettings?.BetfairLoginCredentials?.Password)
            });

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, _betfairSettings?.BetfairEndpoints?.AuthEndpoint);
            requestMessage.AddBaseHeaders();
            requestMessage.Content = content;

            HttpResponseMessage httpResponse = await HttpClientSingleton.Instance.Client.SendAsync(requestMessage);
            // httpResponse.EnsureSuccessStatusCode();
            
            if(httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                try
                {
                    var response = JsonSerializer.Deserialize<BetfairLoginResponse>(httpResponseBody);

                    if((response != null) && response.Status == "SUCCESS")
                    {
                        AuthToken = response?.Token;
                        result.Details = response;
                        result.HttpResponseMessage = httpResponse;
                        result.IsSuccessfull = true;
                    }
                    else
                    {
                        result.IsSuccessfull = false;
                    }


                    // Console.WriteLine(AuthToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            // the server answer is != from 200
            else
            {
                result.IsSuccessfull = false;
                result.HttpResponseMessage = httpResponse;
            }


            return result;

        }

    }
}