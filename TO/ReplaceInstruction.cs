using System.Text;
using Newtonsoft.Json;

namespace bf_bot.TO
{
    public class ReplaceInstruction
    {
        [JsonProperty(PropertyName = "betId")]
        public string BetId { get; set; }

        [JsonProperty(PropertyName = "newPrice")]
        public double NewPrice { get; set;}

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("{0}", "ReplaceInstruction")
                        .AppendFormat(" : BetId={0}", BetId)
                        .AppendFormat(" : NewPrice={0}", NewPrice);

            return sb.ToString();
        }
    }
}
