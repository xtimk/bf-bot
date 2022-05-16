using System.Text;
using System.Text.Json.Serialization;

namespace bf_bot.TO
{
    public class CancelInstruction
    {
        [JsonPropertyName("betId")]
        public string BetId { get; set; }

        [JsonPropertyName("sizeReduction")]
        public double? SizeReduction { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("{0}", "CancelInstruction")
                        .AppendFormat(" : BetId={0}", BetId)
                        .AppendFormat(" : SizeReduction={0}", SizeReduction);

            return sb.ToString();
        }
    }
}
