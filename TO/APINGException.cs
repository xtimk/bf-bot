using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace bf_bot.TO
{
    public class APINGException : System.Exception
    {
        protected APINGException(SerializationInfo info, StreamingContext context)
        {
            this.ErrorDetails = info.GetString("errorDetails");
            this.ErrorCode = info.GetString("errorCode");
            this.RequestUUID = info.GetString("requestUUID");
        }

        [JsonProperty(PropertyName = "errorDetails")]
        public string ErrorDetails { get; set; }

        [JsonProperty(PropertyName = "errorCode")]
        public string ErrorCode { get; set; }	

        [JsonProperty(PropertyName = "requestUUID")]
        public string RequestUUID { get; set; }
    }
}
