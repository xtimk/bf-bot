using bf_bot.Constants;
using bf_bot.Wallets;
using Elasticsearch.Net;
using Nest;

namespace bf_bot
{
    public interface IStrategy
    {
        Task Start();
        bool Init(RunningMode mode, IClient client, IWallet wallet, ElasticClient esClient);
    }
}