using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace bf_bot.TO
{
    public class Exception
    {
        // exception in json-rpc format
        [JsonProperty(PropertyName = "data")]
        public JObject Data { get; set; }		// actual exception details


        // exception in rescript format
        [JsonProperty(PropertyName = "detail")]
        public JObject Detail { get; set; }		// actual exception details

    }
}
