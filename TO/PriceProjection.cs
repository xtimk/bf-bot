using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace bf_bot.TO
{
    public class PriceProjection
    {
        [JsonProperty(PropertyName = "priceData")]
        [JsonPropertyName("priceData")]
        public ISet<PriceData> PriceData { get; set; }

        [JsonProperty(PropertyName = "exBestOffersOverrides")]
        public ExBestOffersOverrides ExBestOffersOverrides { get; set; }
        
        [JsonProperty(PropertyName = "virtualise")]
        public bool? Virtualise { get; set; }

        [JsonProperty(PropertyName = "rolloverStakes")]
        public bool? RolloverStakes { get; set; }

        
    }
}
