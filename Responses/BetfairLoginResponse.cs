using System.Text.Json.Serialization;

namespace bf_bot
{
    public class BetfairLoginResponse : BetfairApiResult
    {
        [JsonPropertyName("token")]
        public string Token { get; set; }
        [JsonPropertyName("product")]
        public string Product { get; set; }
        [JsonPropertyName("status")]
        public string Status { get; set; }
        [JsonPropertyName("error")]
        public string Error { get; set; }
        [JsonPropertyName("lastLoginDate")]
        public string LastLoginDate { get; set; }
    }
}