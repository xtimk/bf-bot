using System.Text;
using Newtonsoft.Json;

namespace bf_bot.TO
{
    public class LimitOrder
    {
        [JsonProperty(PropertyName = "size")]
        public double Size { get; set; }

        [JsonProperty(PropertyName = "price")]
        public double Price { get; set; }

        [JsonProperty(PropertyName = "persistenceType")]
        public PersistenceType PersistenceType { get; set; }

        public override string ToString()
        {
            return new StringBuilder()
                        .AppendFormat("Size={0}", Size)
                        .AppendFormat(" : Price={0}", Price)
                        .AppendFormat(" : PersistenceType={0}", PersistenceType)
                        .ToString();
        }
    }
}
