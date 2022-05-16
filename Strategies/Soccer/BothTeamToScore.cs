using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bf_bot.Json;
using bf_bot.TO;
using Microsoft.Extensions.Logging;

namespace bf_bot.Strategies.Soccer
{
    public class BothTeamToScore : IStrategy
    {
        private readonly IClient _client;
        private readonly ILogger _logger;
        private bool _active = false;
        public BothTeamToScore(IClient client, ILogger logger)
        {
            _logger = logger;
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
            await DoStrategy();
            _logger.LogInformation("BothTeamToScore strategy has stopped.");
        }

        public void Stop()
        {
            _logger.LogInformation("Requested graceful stop of BothTeamToScore strategy.");
            _active = false;
        }

        public async Task DoStrategy()
        {
            var soccerEventIds = await GetSoccerEventTypes();

            var nextGameIs = await GetNextGame(soccerEventIds);       
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

        public async Task<string> GetNextGame(ISet<string> eventypeIds)
        {
            //ListMarketCatalogue: Get next available horse races, parameters:
            var time = new TimeRange();
            time.From = DateTime.Now.AddHours(-1);
            time.To = DateTime.Now.AddDays(1);

            var marketFilter = new MarketFilter();

            marketFilter.EventTypeIds = eventypeIds;
            marketFilter.MarketStartTime = time;
            // marketFilter.MarketCountries = new HashSet<string>() { "GB" };
            // marketFilter.MarketTypeCodes = new HashSet<String>() { "WIN" };

            var marketSort = MarketSort.FIRST_TO_START;
            var maxResults = "1";
            
            //request runner metadata 
            ISet<MarketProjection> marketProjections = new HashSet<MarketProjection>();
            marketProjections.Add(MarketProjection.RUNNER_METADATA);

            _logger.LogInformation("Getting the next available soccer market");

            var marketCatalogues = await _client.listMarketCatalogue(marketFilter, marketProjections, marketSort, maxResults);
            _logger.LogDebug(JsonConvert.Serialize<IList<MarketCatalogue>>(marketCatalogues));
            
            //extract the marketId of the next soccer game
            String marketId = marketCatalogues.First().MarketId;
            return marketId;
        }
    }
}