using System.Runtime.Serialization;
using System.Text.Json.Serialization;
// using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace bf_bot.TO
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PriceData
    {
        SP_AVAILABLE, SP_TRADED,
        EX_BEST_OFFERS, EX_ALL_OFFERS, EX_TRADED,
    }
}
