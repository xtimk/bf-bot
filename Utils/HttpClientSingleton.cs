namespace bf_bot
{
    public sealed class HttpClientSingleton
    {
        public readonly HttpClient Client = new HttpClient();
        private static readonly HttpClientSingleton instance = new HttpClientSingleton();    
        static HttpClientSingleton()    
        {    
        }    
        private HttpClientSingleton()    
        {
            
        }    
        public static HttpClientSingleton Instance    
        {    
            get    
            {    
                return instance;    
            }    
        }      
    }
}