using System.Text.Json.Serialization;
// using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace bf_bot.TO
{
    // [JsonConverter(typeof(StringEnumConverter))]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PriceData
    {
        SP_AVAILABLE, SP_TRADED,
        EX_BEST_OFFERS, EX_ALL_OFFERS, EX_TRADED,
    }

    // public class PriceData
    // {
    //     // public static readonly string SP_AVAILABLE = "SP_AVAILABLE";
    //     // public string SP_TRADED { get; } = "SP_TRADED";
    //     // public string EX_BEST_OFFERS { get; } = "EX_BEST_OFFERS";
    //     public string EX_ALL_OFFERS { get; } = "EX_ALL_OFFERS";
    //     // public string EX_TRADED { get; } = "EX_TRADED";
    // }
}
