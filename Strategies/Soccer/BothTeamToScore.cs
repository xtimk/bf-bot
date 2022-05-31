using bf_bot.Constants;
using bf_bot.Exceptions;
using bf_bot.Json;
using bf_bot.TO;
using bf_bot.Utils;
using bf_bot.Wallets;
using Microsoft.Extensions.Logging;
using Nest;

namespace bf_bot.Strategies.Soccer
{
    public class BothTeamToScore : IStrategy
    {
        // Strategy condition: ToDO: pass this instead of defining it locally
        private MarketBookFilterCondition _condition = new() {
            MaxPrice = 4,
            MinPrice = 1.5,
            MinSize = 10
        };

        // Strategy constants
        private string _marketCatalogueMaxResults = "100";
        private int _timer = 20000;
        private int _wait_result_timer = 60000;
        // --

        private IClient _client;
        private readonly ILogger _logger;
        private bool _active = false;
        private RunningMode _mode;
        private IWallet _wallet;
        private ElasticClient _esClient;
        private readonly AppGuid _sessionGuid;
        public BothTeamToScore(ILogger<BothTeamToScore> logger, AppGuid sessionGuid)
        {
            _logger = logger; // loggerFactory.CreateLogger<BothTeamToScore>();
            _sessionGuid = sessionGuid;
        }

        public bool Init(RunningMode mode, IClient client, IWallet wallet, ElasticClient esClient)
        {
            _wallet = wallet;
            _mode = mode;
            _client = client;
            _esClient = esClient;
            return true;        
        }

        public async Task Start()
        {
            _logger.LogInformation("Starting BothTeamToScore strategy.");
            _logger.LogInformation("Running Mode: " + _mode);

            if (!_client.IsInitialized())
            {
                _logger.LogCritical("Betfair client not initialized. Stopping strategy.");
                return;
            }

            if (!_wallet.IsInitialized())
            {
                _logger.LogCritical("Wallet not initialized. Stopping strategy.");
                return;
            }
            
            if(_mode == RunningMode.REAL)
                throw new NotImplementedException("Bot in real mode is not implemented.");

            var logged_in = await _client.RequestLogin();
            if(!logged_in)
            {
                _logger.LogCritical("User not authenticated, the strategy will now stop.");
                return;
            }

            _active = true;

            _logger.LogInformation("Starting with wallet of type \"" + _wallet.getWalletName() + "\". Balance is " + _wallet.getBalance() + "EUR");
            try
            {
                await DoStrategy();
            }
            catch (BetfairClientException e)
            {
                _logger.LogError("A Betfair Client error has been encountered while executing strategy. Details: " + e.StackTrace);
            }
            catch (System.Exception e)
            {
                _logger.LogError("A generic error has been encountered while executing strategy. Details: " + e.Message);
                _logger.LogError(e.StackTrace);
                _logger.LogError(e.InnerException.ToString());
            }
            finally
            {
                _logger.LogInformation("BothTeamToScore strategy has stopped.");
            }
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
                _logger.LogDebug("Searching match suitable with desired conditions.");

                Thread.Sleep(_timer);

                var soccerEventIds = await GetSoccerEventTypes();

                var marketCatalogues = await GetNextGamesMarketCatalogues(soccerEventIds);

                marketCatalogues = FilterBothTeamToScore(marketCatalogues);                

                var marketBooks = await GetMarketBooks(marketCatalogues);

                marketBooks = FilterOpenMarketBooks(marketBooks);
                if (marketBooks.Count() == 0)
                {
                    _logger.LogDebug("No open market found. Will retry in a moment.");
                    continue;
                }

                marketBooks = FilterMarketBooksByCondition(marketBooks, _condition);
                if (marketBooks.Count() == 0)
                {
                    _logger.LogDebug("No marketbooks matching the strategy conditions. Will retry in a moment.");
                    continue;
                }

                var marketBookToBet = SelectOneMarketBook(marketBooks);
                _logger.LogInformation("Match found!");

                var betPlaced = await PlaceBet(marketBookToBet);
                if(!betPlaced)
                    continue;
                
                await WaitForBetResult(marketBookToBet);

                // just stop at the first iteration for debugging logs after..
                // _active = false;
            }
        }

        public async Task<bool> WaitForBetResult(MarketBook marketBook)
        {
            _logger.LogInformation("Now waiting for bet results.");
            ISet<PriceData> priceData = new HashSet<PriceData>();
            priceData.Add(PriceData.EX_BEST_OFFERS);
            
            var priceProjection = new PriceProjection();
            priceProjection.PriceData = priceData;

            var marketId = marketBook.MarketId;
            var selectionId = marketBook.Runners[0].SelectionId;
            var runnerBook = (IList<MarketBook>)await InvokeBetfairClientMethodAsync(() => _client.listRunnerBook(marketId, selectionId, priceProjection));

            while(runnerBook.First().Runners[0].Status != RunnerStatus.WINNER && runnerBook.First().Runners[0].Status != RunnerStatus.LOSER)
            {
                Thread.Sleep(_wait_result_timer);
                _logger.LogDebug("Checking market updates.");
                runnerBook = (IList<MarketBook>)await InvokeBetfairClientMethodAsync(() => _client.listRunnerBook(marketId, selectionId, priceProjection));
            }

            _logger.LogDebug("Game has ended!");

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
            // I take the best offer (the offer with higher price.)
            var bestExchangeOfferByPrice = marketBook.Runners[0].ExchangePrices.AvailableToBack.OrderByDescending(x => x.Price).FirstOrDefault();

            var price = bestExchangeOfferByPrice.Price;
            var size = bestExchangeOfferByPrice.Size;

            var marketId = marketBook.MarketId;
            var selectionId = marketBook.Runners[0].SelectionId;

            var amountToBet = Math.Round(_wallet.getAmountToBet(price), 2, MidpointRounding.AwayFromZero);
            if(amountToBet > size)
            {
                _logger.LogWarning("Insufficient liquidity in the bet pool.");
                return false;
            }
            if(amountToBet < 2)
            {
                _logger.LogWarning("The betting amount (" + amountToBet + " EUR) is less than the minimum allowed bet (2 EUR). Will automatically bet 2 EUR.");
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

            var bfLink = "https://www.betfair.it/exchange/plus/football/market/" + marketId;
            _wallet.signalPlaceBet(amountToBet, price, bfLink);
            _logger.LogInformation("Betfair link: " + bfLink);

            return true;
        }

        #pragma warning disable
        private async Task<bool> PlaceRealBet(string marketId, long selectionId, double amountToBet, double price)
        {
            _logger.LogCritical("Place real bet here. Not implemented yet!");
            throw new NotImplementedException("Place bet in REAL mode not implemented.");
            return false;
        }

        private bool PlaceFakeBet(string marketId, long selectionId, double amountToBet, double price)
        {
            _logger.LogDebug("Placed bet in TEST mode: " + amountToBet + "EUR @ " + price);
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

            var eventTypes = (IList<EventTypeResult>)await InvokeBetfairClientMethodAsync(() => _client.listEventTypes(marketFilter));

            ISet<string> eventypeIds = new HashSet<string>();   
            foreach (EventTypeResult eventType in eventTypes)
            {
                if (eventType.EventType.Name.Equals("Soccer"))
                {
                    _logger.LogTrace("EventType: " + JsonConvert.Serialize<EventTypeResult>(eventType));
                    eventypeIds.Add(eventType.EventType.Id);
                }
            }
            return eventypeIds;
        }

        public async Task<dynamic> InvokeBetfairClientMethodAsync(Func<dynamic> methodWithParameters) {
            dynamic result = null;
            try
            {
                result = await methodWithParameters();
            }
            catch (BetfairClientException e)
            {
                _logger.LogDebug("Handling BetfairClientException.");
                var isHandled = HandleBetfairEx(e);
                if(isHandled.Result)
                {
                    return await InvokeBetfairClientMethodAsync(methodWithParameters);
                }
            }
            catch (System.Exception e)
            {
                _logger.LogWarning("Betfair client error not handled: " + e);
            }
            return result;
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

            _logger.LogDebug("Getting the next " + maxResults + " available soccer markets");

            var marketCatalogues = (IList<MarketCatalogue>)await InvokeBetfairClientMethodAsync(() => _client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults));

            return marketCatalogues.ToList();
        }

        public List<MarketBook> FilterMarketBooksByCondition(List<MarketBook> marketBooks, MarketBookFilterCondition condition)
        {
            var filteredMarketBooks = new List<MarketBook>();
            foreach (var item in marketBooks)
            {
                try
                {
                    var bestExchangeOfferByPrice = item.Runners[0].ExchangePrices.AvailableToBack.OrderByDescending(x => x.Price).FirstOrDefault();
                    if (bestExchangeOfferByPrice.Price < condition.MaxPrice &&
                        bestExchangeOfferByPrice.Price > condition.MinPrice &&
                        bestExchangeOfferByPrice.Size > condition.MinSize)
                    {
                        filteredMarketBooks.Add(item);
                    }
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

            var marketBook = (IList<MarketBook>)await InvokeBetfairClientMethodAsync(() => _client.listMarketBook(marketCatalogues.Select(x => x.MarketId).ToList(), priceProjection));
            
            return marketBook.ToList();
        }
        public async Task<bool> HandleBetfairEx(BetfairClientException e)
        {
            if(e.Body != null)
            {
                try
                {
                    dynamic errorBody = e.Body;
                    var errorDetails = Newtonsoft.Json.JsonConvert.DeserializeObject(errorBody);
                    var errorCode = errorDetails.detail.APINGException.errorCode;
                    if(errorCode == "INVALID_SESSION_INFORMATION")
                    {
                        _logger.LogDebug("Authentication token has expired.");
                        await _client.RequestLogin();
                        return true;
                    }
                    else
                    {
                        _logger.LogError("A Betfair client error with errorCode not handled has been encountered. Message: " + e.BFMessage + ". Body: " + e.Body);
                        return false;
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogError("An error has been encountered while handling a betfair client error. Details: " + ex);
                    return false;
                }
            }
            else
            {
                _logger.LogError("A betfair client error has been encountered while executing strategy. Details: " + e.Message);
                return false;
            }
        }
    }
}