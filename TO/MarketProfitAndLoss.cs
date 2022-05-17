﻿using System.Text;
using Newtonsoft.Json;

namespace bf_bot.TO
{
    public class MarketProfitAndLoss
    {
        [JsonProperty(PropertyName = "marketId")]
        public string MarketId { get; set; }

        [JsonProperty(PropertyName = "commissionApplied")]
        public double CommissionApplied { get; set; }

        [JsonProperty(PropertyName = "profitAndLosses")]
        public List<RunnerProfitAndLoss> ProfitAndLosses { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("{0}", "MarketProfitAndLoss")
                        .AppendFormat(" : MarketId={0}", MarketId)
                        .AppendFormat(" : CommissionApplied={0}", CommissionApplied)
                        .ToString();

            if (ProfitAndLosses != null && ProfitAndLosses.Count > 0)
            {
                int idx = 0;
                foreach (var profitandloss in ProfitAndLosses)
                {
                    sb.AppendFormat(" : ProfitAndLosses[{0}]={1}", idx++, profitandloss);
                }
            }

            return sb.ToString();
        }
    }
}
