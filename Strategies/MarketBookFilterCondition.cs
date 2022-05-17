using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bf_bot.Strategies
{
    public class MarketBookFilterCondition
    {
        public double MaxPrice { get; set; }
        public double MinPrice { get; set; }
        public double MinSize { get; set; }
    }
}