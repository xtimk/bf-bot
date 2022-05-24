using bf_bot.Constants;
using bf_bot.Exceptions;
using bf_bot.Json;
using bf_bot.TO;
using bf_bot.Wallets;
using Microsoft.Extensions.Logging;

namespace bf_bot.Strategies.Soccer
{
    public class BothTeamToScore : IStrategy
    {
        private string _marketCatalogueMaxResults = "100";
        private MarketBookFilterCondition _condition = new() {
            MaxPrice = 4,
            MinPrice = 1.7,
            MinSize = 10
        };

        private int _timer = 3000;
        private IClient _client;
        private readonly ILogger<BothTeamToScore> _logger;
        private bool _active = false;
        private readonly RunningMode _mode;
        private readonly IWallet _wallet;
        public BothTeamToScore(RunningMode mode, IClient client, ILoggerFactory loggerFactory, IWallet wallet)
        {
            _wallet = wallet;
            _mode = mode;
            _logger = loggerFactory.CreateLogger<BothTeamToScore>();
            _client = client;
        }

        public async Task Start()
        {
            _logger.LogInformation("Starting BothTeamToScore strategy.");
            _logger.LogInformation("Running Mode: " + _mode);

            // first of all authenticate the client.
            var logged_in = await _client.RequestLogin();
            if(!logged_in)
            {
                _logger.LogCritical("User not authenticated, the strategy will now stop.");
                return;
            }

            _active = true;

            // init wallet... ToDo: check if we can init in a better way..
            _wallet.Init(1000, 2);

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
                Thread.Sleep(_timer);

                var soccerEventIds = await GetSoccerEventTypes();

                var marketCatalogues = await GetNextGamesMarketCatalogues(soccerEventIds);

                marketCatalogues = FilterBothTeamToScore(marketCatalogues);                

                var marketBooks = await GetMarketBooks(marketCatalogues);

                marketBooks = FilterOpenMarketBooks(marketBooks);
                if (marketBooks.Count() == 0)
                {
                    _logger.LogInformation("No open market found. Will retry in a moment.");
                    continue;
                }

                marketBooks = FilterMarketBooksByCondition(marketBooks, _condition);
                if (marketBooks.Count() == 0)
                {
                    _logger.LogInformation("No marketbooks matching the strategy conditions. Will retry in a moment.");
                    continue;
                }

                var marketBookToBet = SelectOneMarketBook(marketBooks);

                var betPlaced = await PlaceBet(marketBookToBet);
                if(!betPlaced)
                    continue;

                await WaitForBetResult(marketBookToBet);

                // just stop at the first iteration for debugging logs after..
                _active = false;
            }
        }

        public async Task<bool> WaitForBetResult(MarketBook marketBook)
        {
            ISet<PriceData> priceData = new HashSet<PriceData>();
            priceData.Add(PriceData.EX_BEST_OFFERS);
            
            var priceProjection = new PriceProjection();
            priceProjection.PriceData = priceData;

            var marketId = marketBook.MarketId;
            var selectionId = marketBook.Runners[0].SelectionId;

            var runnerBook = await _client.listRunnerBook(marketId, selectionId, priceProjection);
            _logger.LogTrace(JsonConvert.Serialize<IList<MarketBook>>(runnerBook));

            while(runnerBook.First().Runners[0].Status != RunnerStatus.WINNER && runnerBook.First().Runners[0].Status != RunnerStatus.LOSER)
            {
                Thread.Sleep(60000 * 5);
                runnerBook = await _client.listRunnerBook(marketId, selectionId, priceProjection);
                _logger.LogTrace(JsonConvert.Serialize<IList<MarketBook>>(runnerBook));
            }

            if(runnerBook.First().Runners[0].Status == RunnerStatus.WINNER)
            {
                _logger.LogInformation("Bet winned!");
                _wallet.signalWin();
                return true;
            }
            else
            {
                _logger.LogInformation("Bet is lost.");
                _wallet.signalLose();
                return false;
            }
        }
        public async Task<bool> PlaceBet(MarketBook marketBook)
        {

            // Runners[0] stays for BothTeamToScore YES
            // I take first() since im using EX_BEST_OFFERS, that returns a single offer.
            var price = marketBook.Runners[0].ExchangePrices.AvailableToBack.First().Price;
            var size = marketBook.Runners[0].ExchangePrices.AvailableToBack.First().Size;

            var marketId = marketBook.MarketId;
            var selectionId = marketBook.Runners[0].SelectionId;

            var amountToBet = Math.Round(_wallet.getAmountToBet(price), 2, MidpointRounding.AwayFromZero);
            if(amountToBet > size)
            {
                _logger.LogInformation("Insufficient liquidity in the bet pool.");
                return false;
            }
            if(amountToBet < 2)
            {
                _logger.LogWarning("The betting amount <" + amountToBet + "> is less than the minimal bet (2 EUR). Will automatically bet 2 EUR.");
                amountToBet = 2;
            }

            var betPlaced = false;
            if(_mode == RunningMode.REAL)
            {
                betPlaced = await PlaceRealBet(marketId, selectionId, amountToBet, price);
            }
            else
            {
                betPlaced = PlaceFakeBet(marketId, selectionId, amountToBet, price);
            }

            if(!betPlaced)
            {
                _logger.LogWarning("Bet not placed!");
                return false;
            }

            _wallet.signalPlaceBet(amountToBet);

            _logger.LogInformation("Placed bet " + amountToBet + "EUR @ " + price);
            _logger.LogInformation("Betfair link: https://www.betfair.it/exchange/plus/football/market/" + marketId);

            return true;
        }

#pragma warning disable
        private async Task<bool> PlaceRealBet(string marketId, long selectionId, double amountToBet, double price)
        {
            _logger.LogCritical("Place real bet here. Not implemented yet!");
            return false;
        }

        private bool PlaceFakeBet(string marketId, long selectionId, double amountToBet, double price)
        {
            _logger.LogInformation("Placed bet in TEST mode.");
            return true;
        }

        public MarketBook SelectOneMarketBook(List<MarketBook> marketBooks)
        {
            var result = marketBooks.First();
            _logger.LogTrace(JsonConvert.Serialize<MarketBook>(result));

            return result;
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
            var maxResults = _marketCatalogueMaxResults;
            
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
                    // get first runner available to back = Bet on Both Teams to Score yes.
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
            _logger.LogTrace("Marketbooks matching conditions: " + JsonConvert.Serialize<IList<MarketBook>>(filteredMarketBooks));
            return filteredMarketBooks;
        }

        public List<MarketBook> FilterOpenMarketBooks(List<MarketBook> marketBooks)
        {
            return marketBooks.Where(x => x.Status == MarketStatus.OPEN).ToList();
        }
        public async Task<List<MarketBook>> GetMarketBooks(List<MarketCatalogue> marketCatalogues)
        {
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