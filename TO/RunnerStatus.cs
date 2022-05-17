using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace bf_bot.TO
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RunnerStatus
    {
        ACTIVE, WINNER, LOSER, REMOVED_VACANT, REMOVED
    }
}
