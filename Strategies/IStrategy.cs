using bf_bot.Constants;
using bf_bot.Wallets;
using Elasticsearch.Net;
using Nest;

namespace bf_bot
{
    public interface IStrategy
    {
        /// <summary>
        /// Starts the strategy
        /// </summary>
        Task Start();
        /// <summary>
        /// Initialize strategy params
        /// </summary>
        bool Init(RunningMode mode, IClient client, IWallet wallet, ElasticClient esClient);
        /// <summary>
        /// Gets the wallet name
        /// </summary>
        string getStrategyName();
    }
}