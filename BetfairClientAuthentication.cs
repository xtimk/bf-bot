using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using bf_bot.Extensions;

namespace bf_bot
{
    public static class BetfairClientAuthentication
    {
        // private readonly BetfairLoginCredentials _credentials;
        // private readonly string _endpoint;

        // public BetfairClientAuthentication(BetfairLoginCredentials credentials, string endpoint)
        // {
        //     _endpoint = endpoint;
        //     _credentials = credentials;
        // }
        // public static async Task<BetfairApiResult<BetfairLoginResponse>> Login()
        // {
        //     BetfairApiResult<BetfairLoginResponse> result = new BetfairApiResult<BetfairLoginResponse>
        //     {
        //         IsSuccessfull = false,
        //         Details = null
        //     };

        //     AppSettings settings = AppSettings.Instance;

        //     var content = new FormUrlEncodedContent(new[]
        //     {
        //         new KeyValuePair<string, string>("username", _credentials.Username),
        //         new KeyValuePair<string, string>("password", _credentials.Password)
        //     });

        //     HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, _endpoint);
        //     requestMessage.AddBaseHeaders();
        //     requestMessage.Content = content;

        //     HttpResponseMessage httpResponse = await HttpClientSingleton.Instance.Client.SendAsync(requestMessage);
        //     // httpResponse.EnsureSuccessStatusCode();
            
        //     if(httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
        //     {
        //         string httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
        //         try
        //         {
        //             var response = JsonSerializer.Deserialize<BetfairLoginResponse>(httpResponseBody);

        //             if((response != null) && response.Status == "SUCCESS")
        //             {
        //                 // AuthToken = response?.Token;
        //                 result.Details = response;
        //                 result.HttpResponseMessage = httpResponse;
        //                 result.IsSuccessfull = true;
        //             }
        //             else
        //             {
        //                 result.IsSuccessfull = false;
        //             }


        //             // Console.WriteLine(AuthToken);
        //         }
        //         catch (Exception e)
        //         {
        //             Console.WriteLine(e);
        //         }
        //     }
        //     // the server answer is != from 200
        //     else
        //     {
        //         result.IsSuccessfull = false;
        //         result.HttpResponseMessage = httpResponse;
        //     }


        //     return result;

        // }
    }
}