using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace bf_bot.TO
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MarketStatus
    {
        INACTIVE, OPEN, SUSPENDED, CLOSED
    }
}
