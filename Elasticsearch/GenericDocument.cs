using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nest;

namespace bf_bot.Elasticsearch
{
    public abstract class GenericDocument
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string DocType { get; set; }
        public Guid SessionGuid { get; set; }
        public string StrategyName { get; set; }
        public string WalletName { get; set; }
    }
}