using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace bf_bot.TO
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ExecutionReportStatus
    {
        SUCCESS,
        FAILURE,
        PROCESSED_WITH_ERRORS,
        TIMEOUT
    }
}
