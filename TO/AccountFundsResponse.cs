using System.Text;
using System.Text.Json.Serialization;

namespace bf_bot.TO
{
    public class AccountFundsResponse
    {
        [JsonPropertyName("availableToBetBalance")]
        public double AvailableToBetBalance { get; set; }

        [JsonPropertyName("exposure")]
        public double Exposure { get; set; }

        [JsonPropertyName("retainedCommission")]
        public double RetainedCommission { get; set; }

        [JsonPropertyName("exposureLimit")]
        public double ExposureLimit { get; set; }

        [JsonPropertyName("discountRate")]
        public double DiscountRate { get; set; }
        
        [JsonPropertyName("pointsBalance")]
        public int PointsBalance { get; set; }

        [JsonPropertyName("marketCount")]
        public int MarketCount { get; set; }

        public override string ToString()
        {
            return new StringBuilder().AppendFormat("{0}", "AccountFundsResponse")
                        .AppendFormat(" : AvailableToBetBalance={0}", AvailableToBetBalance)
                        .AppendFormat(" : Exposure={0}", Exposure)
                        .AppendFormat(" : RetainedCommission={0}", RetainedCommission)
                        .AppendFormat(" : ExposureLimit={0}", ExposureLimit)
                        .AppendFormat(" : DiscountRate={0}", DiscountRate)
                        .AppendFormat(" : PointsBalance={0}", PointsBalance)
                        .AppendFormat(" : MarketCount={0}", MarketCount)
                        .ToString();
        }
    }
}
