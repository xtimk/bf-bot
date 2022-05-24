using bf_bot.TO;

namespace bf_bot
{
    public interface IClient
    {
        void Init(BetfairClientInitializer betfairSettings);
        /**
        * calls indentitysso for authenticating user and sets session token
        * 
        * */
        Task<bool> RequestLogin();
        /**
         * calls api-ng to get a list of events
         * 
         * */
        Task<IList<EventTypeResult>> listEventTypes(MarketFilter marketFilter, string locale = null);
        
        /**
         * calls api-ng to get a list of market catalogues
         * */
        Task<IList<MarketCatalogue>> listMarketCatalogue(MarketFilter marketFilter, ISet<MarketProjection> marketProjections, MarketSort marketSort, string maxResult = "1", string locale = null);

        /**
         * calls api-ng to get more detailed info about the specified markets
         * */
        Task<IList<MarketBook>> listMarketBook(IList<string> marketIds, PriceProjection priceProjection, OrderProjection? orderProjection = null, MatchProjection? matchProjection = null, string currencyCode = null, string locale = null);

        /**
         * calls api-ng to get detailed info about specific runner book
         * */
        Task<IList<MarketBook>> listRunnerBook(string marketId, long selectionId, PriceProjection priceProjection, OrderProjection? orderProjection = null, MatchProjection? matchProjection = null, string currencyCode = null, string locale = null);


        /**
         * places a bet
         * */
        Task<PlaceExecutionReport> placeOrders(string marketId, string customerRef, IList<PlaceInstruction> placeInstructions, string locale = null);

        /**
         * Lists market profit and loss
         * */
        Task<IList<MarketProfitAndLoss>> listMarketProfitAndLoss(IList<string> marketIds, bool includeSettledBets = false, bool includeBspBets = false, bool netOfCommission = false);

        /**
         * Lists current orders 
         * */
        Task<CurrentOrderSummaryReport> listCurrentOrders(ISet<String> betIds, ISet<String> marketIds, OrderProjection? orderProjection = null, TimeRange placedDateRange = null, OrderBy? orderBy = null, SortDir? sortDir = null, int? fromRecord = null, int? recordCount = null);

        /**
         * Lists cleared orders
         * */
        Task<ClearedOrderSummaryReport> listClearedOrders(BetStatus betStatus, ISet<string> eventTypeIds = null, ISet<string> eventIds = null, ISet<string> marketIds = null, ISet<RunnerId> runnerIds = null, ISet<string> betIds = null, Side? side = null, TimeRange settledDateRange = null, GroupBy? groupBy = null, bool? includeItemDescription = null, String locale = null, int? fromRecord = null, int? recordCount = null);

        /**
         * Cancels a bet, or decreases its size
         * */
        Task<CancelExecutionReport> cancelOrders(string marketId, IList<CancelInstruction> instructions, string customerRef);

        /**
         * Replaces a bet: changes the price
         * */
        Task<ReplaceExecutionReport> replaceOrders(String marketId, IList<ReplaceInstruction> instructions, String customerRef);

        /**
         * updates a bet
         * */
        Task<UpdateExecutionReport> updateOrders(String marketId, IList<UpdateInstruction> instructions, String customerRef);


    }
}