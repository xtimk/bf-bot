using bf_bot.Exceptions;
using bf_bot.Json;
using bf_bot.TO;
using Microsoft.Extensions.Logging;

namespace bf_bot.Strategies.Soccer
{
    public class BothTeamToScore : IStrategy
    {
        private IClient _client;
        private readonly ILogger<BothTeamToScore> _logger;
        private bool _active = false;
        public BothTeamToScore(IClient client, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BothTeamToScore>();
            _client = client;
        }

        public async Task Start()
        {
            _logger.LogInformation("Starting BothTeamToScore strategy.");

            // first of all authenticate the client.
            var logged_in = await _client.RequestLogin();
            if(!logged_in)
            {
                _logger.LogCritical("User not authenticated, the strategy will now stop.");
                return;
            }

            _active = true;

            try
            {
                await DoStrategy();
            }
            catch (BetfairClientException e)
            {
                _logger.LogError("A client error has been encountered while executing strategy. Details: " + e.Message);
            }
            catch (System.Exception e)
            {
                _logger.LogError("A generic error has been encountered while executing strategy. Details: " + e.Message);
            }
            _logger.LogInformation("BothTeamToScore strategy has stopped.");
        }

        public void Stop()
        {
            _logger.LogInformation("Requested graceful stop of BothTeamToScore strategy.");
            _active = false;
        }

        public async Task DoStrategy()
        {
            while(_active)
            {
                Thread.Sleep(3000);

                var soccerEventIds = await GetSoccerEventTypes();

                var marketCatalogues = await GetNextGamesMarketCatalogues(soccerEventIds);

                marketCatalogues = FilterBothTeamToScore(marketCatalogues);                

                var marketBooks = await GetMarketBooks(marketCatalogues);
                _logger.LogTrace(JsonConvert.Serialize<List<MarketBook>>(marketBooks));

                marketBooks = FilterOpenMarketBooks(marketBooks);
                if (marketBooks.Count() == 0)
                {
                    _logger.LogInformation("No open market found. Will retry in a moment.");
                    continue;
                }

                var condition = new MarketBookFilterCondition{
                    MaxPrice = 3,
                    MinPrice = 1.7,
                    MinSize = 10
                };

                marketBooks = FilterMarketBooksByCondition(marketBooks, condition);
                if (marketBooks.Count() == 0)
                {
                    _logger.LogInformation("No marketbooks matching the conditions. Will retry in a moment.");
                    continue;
                }
                _logger.LogTrace("Marketbooks matching conditions: " + JsonConvert.Serialize<IList<MarketBook>>(marketBooks));
                _logger.LogInformation("First marketbook matching conditions: " + JsonConvert.Serialize<MarketBook>(marketBooks.First()));
            }
        }

        public List<MarketCatalogue> FilterBothTeamToScore(List<MarketCatalogue> marketCatalogues)
        {
            return marketCatalogues.Where(x => x.MarketName == "Both teams to Score?").ToList();
        }

        public async Task<ISet<string>> GetSoccerEventTypes()
        {
            var marketFilter = new MarketFilter();
            var eventTypes = await _client.listEventTypes(marketFilter);
            ISet<string> eventypeIds = new HashSet<string>();   
            foreach (EventTypeResult eventType in eventTypes)
            {
                if (eventType.EventType.Name.Equals("Soccer"))
                {
                    _logger.LogDebug("EventType: " + JsonConvert.Serialize<EventTypeResult>(eventType));
                    eventypeIds.Add(eventType.EventType.Id);
                }
            }
            return eventypeIds;
        }

        public async Task<List<MarketCatalogue>> GetNextGamesMarketCatalogues(ISet<string> eventypeIds)
        {
            //Set a timerange where to search
            var time = new TimeRange();
            time.From = DateTime.Now.AddHours(-1);
            time.To = DateTime.Now.AddDays(1);
            var marketFilter = new MarketFilter();
            marketFilter.EventTypeIds = eventypeIds;
            marketFilter.MarketStartTime = time;
            var marketSort = MarketSort.FIRST_TO_START;
            var maxResults = "100";
            
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>();
            marketProjections.Add(MarketProjection.EVENT_TYPE);
            marketProjections.Add(MarketProjection.EVENT);
            marketProjections.Add(MarketProjection.COMPETITION);

            _logger.LogInformation("Getting the next " + maxResults + " available soccer markets");

            var marketCatalogues = await _client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);

            _logger.LogTrace(JsonConvert.Serialize<IList<MarketCatalogue>>(marketCatalogues));

            return marketCatalogues.ToList();
        }

        public List<MarketBook> FilterMarketBooksByCondition(List<MarketBook> marketBooks, MarketBookFilterCondition condition)
        {
            var filteredMarketBooks = new List<MarketBook>();
            foreach (var item in marketBooks)
            {
                try
                {
                    var filteredList = item.Runners[0].ExchangePrices.AvailableToBack
                        .Where(
                            x => x.Price < condition.MaxPrice && 
                            x.Price > condition.MinPrice && 
                            x.Size > condition.MinSize
                        );
                    if (filteredList.Count() > 0)
                        filteredMarketBooks.Add(item);
                }
                catch (System.Exception)
                {
                    _logger.LogDebug("No ExchangePrices found for item " + JsonConvert.Serialize<MarketBook>(item));
                }
            }
            return filteredMarketBooks;
        }

        public List<MarketBook> FilterOpenMarketBooks(List<MarketBook> marketBooks)
        {
            return marketBooks.Where(x => x.Status == MarketStatus.OPEN).ToList();
        }
        public async Task<List<MarketBook>> GetMarketBooks(List<MarketCatalogue> marketCatalogues)
        {
            var listToReturn = new List<string>();

            ISet<PriceData> priceData = new HashSet<PriceData>();
            priceData.Add(PriceData.EX_BEST_OFFERS);
            
            var priceProjection = new PriceProjection();
            priceProjection.PriceData = priceData;

            var marketBook = await _client.listMarketBook(marketCatalogues.Select(x => x.MarketId).ToList(), priceProjection);
            _logger.LogTrace(JsonConvert.Serialize<IList<MarketBook>>(marketBook));
            
            return marketBook.ToList();
        }
    }
}