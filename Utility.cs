using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace bf_bot
{
    public static class Utility
    {
        public static string PrettyJsonObject(object o)
        {
            return JsonSerializer.Serialize(o, new JsonSerializerOptions{
                WriteIndented = true
            });
        }

        public static BetfairClientInitializer CreateInitializer()
        {
            AppSettings app = AppSettings.Instance;
            BetfairLoginCredentials loginCredentials = new BetfairLoginCredentials
            {
                Username = app.Username,
                Password = app.Password,
                AppKey = app.AppKey
            };

            BetfairEndpoints betfairEndpoints = new BetfairEndpoints
            {
                BettingEndpoint = app.BettingEndpoint,
                AccountEndpoint = app.AccountEndpoint,
                AuthEndpoint = app.AuthEndPoint
            };

            BetfairClientInitializer initializer = new BetfairClientInitializer
            {
                BetfairLoginCredentials = loginCredentials,
                BetfairEndpoints = betfairEndpoints
            };

            return initializer;
        }

        public static bool AreAllPropNotNull(object myObject)
        {
            foreach(PropertyInfo pi in myObject.GetType().GetProperties())
            {
                if(pi == null)
                    return false;
            }
            return true;
        }
    }
}