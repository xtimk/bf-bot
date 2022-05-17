using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace bf_bot.TO
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum GroupBy
    {
        EVENT_TYPE,
        EVENT,
        MARKET,
        SIDE,
        BET
    }
}
