using Elasticsearch.Net;

namespace bf_bot.Elasticsearch
{
    public class SignalBetDocument : GenericDocument
    {
        public int Step { get; set; }
        public double Balance { get; set; }
        public double BetAmount { get; set; }
        public double Price { get; set; }
        public string BetfairLink { get; set; }
    }
}