using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bf_bot.Elasticsearch
{
    public class SignalLoseDocument : GenericDocument
    {
        public int Step { get; set; }
        public double Balance { get; set; }
        public string BetfairLink { get; set; }
        public string BetResult { get; set; } = "LOSE";
        public Guid CycleGuid { get; set; }
    }
}