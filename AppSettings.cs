using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;
// using Microsoft.Extensions.Configuration.Binder;

namespace bf_bot
{

    public sealed class AppSettings    
    {    
        public string Username { get; set; }
        public string Password { get; set; }
        public string AppKey { get; set; }
        public string  AuthEndPoint { get; set; }
        private static readonly AppSettings instance = new AppSettings();    
        static AppSettings()    
        {    
        }    
        private AppSettings()    
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);
            
            IConfiguration config = builder.Build();

            var betfairLoginSettings = config.GetSection("BetfairLoginCredentials");

            Username = betfairLoginSettings.GetValue<string>("Username");
            Password = betfairLoginSettings.GetValue<string>("Password");
            AppKey = betfairLoginSettings.GetValue<string>("AppKey");
            AuthEndPoint = betfairLoginSettings.GetValue<string>("AuthEndPoint");
        }    
        public static AppSettings Instance    
        {    
            get    
            {    
                return instance;    
            }    
        }    
    }
}