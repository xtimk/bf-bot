using System.Text;
using System.Text.Json;
using bf_bot.Extensions;
using bf_bot.TO;
using bf_bot.Json;
using Microsoft.Extensions.Logging;
using bf_bot.Exceptions;

namespace bf_bot
{
    public class BetfairRpcClientRpc : IClient
    {
        public string AuthToken { get; set; }
        protected BetfairClientInitializer _betfairSettings;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        public void Init(BetfairClientInitializer betfairSettings)
        {
            _logger.LogInformation("Initializing client.");
            if(Utility.AreAllPropNotNull(betfairSettings))
            {
                _betfairSettings = betfairSettings;
            }
        }
        public BetfairRpcClientRpc(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<BetfairRpcClientRpc>();
        }

        public BetfairRpcClientRpc(ILoggerFactory loggerFactory, BetfairClientInitializer betfairSettings)
        {
            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<BetfairRpcClientRpc>();
            if(Utility.AreAllPropNotNull(betfairSettings))
            {
                _betfairSettings = betfairSettings;
            }
        }

        public async Task<IList<EventTypeResult>> listEventTypes(MarketFilter marketFilter, string locale = null)
        {
            var args = new Dictionary<string, object>();
            args[Constants.BetfairConstants.FILTER] = marketFilter;
            args[Constants.BetfairConstants.LOCALE] = locale;
            return await Invoke<List<EventTypeResult>>(Constants.BetfairConstants.LIST_EVENT_TYPES_METHOD, args);
        }

        public async Task<IList<MarketCatalogue>> listMarketCatalogue(MarketFilter marketFilter, ISet<MarketProjection> marketProjections, MarketSort marketSort, string maxResult = "1", string locale = null)
        {
            var args = new Dictionary<string, object>();
            args[Constants.BetfairConstants.FILTER] = marketFilter;
            args[Constants.BetfairConstants.MARKET_PROJECTION] = marketProjections;
            args[Constants.BetfairConstants.SORT] = marketSort; // JsonConvert.Serialize<MarketSort>(marketSort).Replace("\u0022", "");
            args[Constants.BetfairConstants.MAX_RESULTS] = maxResult;
            args[Constants.BetfairConstants.LOCALE] = locale;
            return await Invoke<List<MarketCatalogue>>(Constants.BetfairConstants.LIST_MARKET_CATALOGUE_METHOD, args);
        }

        public async Task<IList<MarketBook>> listMarketBook(IList<string> marketIds, PriceProjection priceProjection, OrderProjection? orderProjection = null, MatchProjection? matchProjection = null, string currencyCode = null, string locale = null)
        {
            var args = new Dictionary<string, object>();
            args[Constants.BetfairConstants.MARKET_IDS] = marketIds;
            args[Constants.BetfairConstants.PRICE_PROJECTION] = priceProjection;// JsonConvert.Serialize<PriceProjection>(priceProjection); // .Replace("\u0022", "");
            args[Constants.BetfairConstants.ORDER_PROJECTION] = orderProjection;
            args[Constants.BetfairConstants.MATCH_PROJECTION] = matchProjection;
            args[Constants.BetfairConstants.LOCALE] = locale;
            args[Constants.BetfairConstants.CURRENCY_CODE] = currencyCode;
            return await Invoke<List<MarketBook>>(Constants.BetfairConstants.LIST_MARKET_BOOK_METHOD, args);
        }
        public async Task<IList<MarketBook>> listRunnerBook(string marketId, long selectionId, PriceProjection priceProjection, OrderProjection? orderProjection = null, MatchProjection? matchProjection = null, string currencyCode = null, string locale = null)
        {
            var args = new Dictionary<string, object>();
            args[Constants.BetfairConstants.MARKET_ID] = marketId;
            args[Constants.BetfairConstants.SELECTION_ID] = selectionId;
            args[Constants.BetfairConstants.PRICE_PROJECTION] = priceProjection;
            args[Constants.BetfairConstants.ORDER_PROJECTION] = orderProjection;
            args[Constants.BetfairConstants.MATCH_PROJECTION] = matchProjection;
            args[Constants.BetfairConstants.LOCALE] = locale;
            args[Constants.BetfairConstants.CURRENCY_CODE] = currencyCode;
            return await Invoke<List<MarketBook>>(Constants.BetfairConstants.LIST_RUNNER_BOOK_METHOD, args);
        }

        public async Task<PlaceExecutionReport> placeOrders(string marketId, string customerRef, IList<PlaceInstruction> placeInstructions, string locale = null)
        {
            var args = new Dictionary<string, object>();

            args[Constants.BetfairConstants.MARKET_ID] = marketId;
            args[Constants.BetfairConstants.INSTRUCTIONS] = placeInstructions;
            args[Constants.BetfairConstants.CUSTOMER_REFERENCE] = customerRef;
            args[Constants.BetfairConstants.LOCALE] = locale;

            return await Invoke<PlaceExecutionReport>(Constants.BetfairConstants.PLACE_ORDERS_METHOD, args);
        }
        
        private static System.Exception ReconstituteException(bf_bot.TO.Exception ex)
        {
            var data = ex.Detail;
        
            // API-NG exception -- it must have "data" element to tell us which exception
            var exceptionName = data.Property("exceptionname").Value.ToString();
            var exceptionData = data.Property(exceptionName).Value.ToString();
            return JsonConvert.Deserialize<APINGException>(exceptionData);
            
        }

        public async Task<IList<MarketProfitAndLoss>> listMarketProfitAndLoss(IList<string> marketIds, bool includeSettledBets = false, bool includeBspBets = false, bool netOfCommission = false)
        {
            var args = new Dictionary<string, object>();
            args[Constants.BetfairConstants.MARKET_IDS] = marketIds;
            args[Constants.BetfairConstants.INCLUDE_SETTLED_BETS] = includeSettledBets;
            args[Constants.BetfairConstants.INCLUDE_BSP_BETS] = includeBspBets;
            args[Constants.BetfairConstants.NET_OF_COMMISSION] = netOfCommission;

            return await Invoke<List<MarketProfitAndLoss>>(Constants.BetfairConstants.LIST_MARKET_PROFIT_AND_LOST_METHOD, args);
        }

        public async Task<CurrentOrderSummaryReport> listCurrentOrders(ISet<String> betIds, ISet<String> marketIds, OrderProjection? orderProjection = null, TimeRange placedDateRange = null, OrderBy? orderBy = null, SortDir? sortDir = null, int? fromRecord = null, int? recordCount = null)
        {
            var args = new Dictionary<string, object>();
            args[Constants.BetfairConstants.BET_IDS] = betIds;
            args[Constants.BetfairConstants.MARKET_IDS] = marketIds;
            args[Constants.BetfairConstants.ORDER_PROJECTION] = orderProjection;
            args[Constants.BetfairConstants.PLACED_DATE_RANGE] = placedDateRange;
            args[Constants.BetfairConstants.ORDER_BY] = orderBy;
            args[Constants.BetfairConstants.SORT_DIR] = sortDir;
            args[Constants.BetfairConstants.FROM_RECORD] = fromRecord;
            args[Constants.BetfairConstants.RECORD_COUNT] = recordCount;

            return await Invoke<CurrentOrderSummaryReport>(Constants.BetfairConstants.LIST_CURRENT_ORDERS_METHOD, args);
        }

        public async Task<ClearedOrderSummaryReport> listClearedOrders(BetStatus betStatus, ISet<string> eventTypeIds = null, ISet<string> eventIds = null, ISet<string> marketIds = null, ISet<RunnerId> runnerIds = null, ISet<string> betIds = null, Side? side = null, TimeRange settledDateRange = null, GroupBy? groupBy = null, bool? includeItemDescription = null, String locale = null, int? fromRecord = null, int? recordCount = null)
        {
            var args = new Dictionary<string, object>();
            args[Constants.BetfairConstants.BET_STATUS] = betStatus;
            args[Constants.BetfairConstants.EVENT_TYPE_IDS] = eventTypeIds;
            args[Constants.BetfairConstants.EVENT_IDS] = eventIds;
            args[Constants.BetfairConstants.MARKET_IDS] = marketIds;
            args[Constants.BetfairConstants.RUNNER_IDS] = runnerIds;
            args[Constants.BetfairConstants.BET_IDS] = betIds;
            args[Constants.BetfairConstants.SIDE] = side;
            args[Constants.BetfairConstants.SETTLED_DATE_RANGE] = settledDateRange;
            args[Constants.BetfairConstants.GROUP_BY] = groupBy;
            args[Constants.BetfairConstants.INCLUDE_ITEM_DESCRIPTION] = includeItemDescription;
            args[Constants.BetfairConstants.LOCALE] = locale;
            args[Constants.BetfairConstants.FROM_RECORD] = fromRecord;
            args[Constants.BetfairConstants.RECORD_COUNT] = recordCount;

            return await Invoke<ClearedOrderSummaryReport>(Constants.BetfairConstants.LIST_CLEARED_ORDERS_METHOD, args);
        }

        public async Task<CancelExecutionReport> cancelOrders(string marketId, IList<CancelInstruction> instructions, string customerRef)
        {
            var args = new Dictionary<string, object>();
            args[Constants.BetfairConstants.MARKET_ID] = marketId;
            args[Constants.BetfairConstants.INSTRUCTIONS] = instructions;
            args[Constants.BetfairConstants.CUSTOMER_REFERENCE] = customerRef;

            return await Invoke<CancelExecutionReport>(Constants.BetfairConstants.CANCEL_ORDERS_METHOD, args);
        }

        public async Task<ReplaceExecutionReport> replaceOrders(String marketId, IList<ReplaceInstruction> instructions, String customerRef)
        {
            var args = new Dictionary<string, object>();
            args[Constants.BetfairConstants.MARKET_ID] = marketId;
            args[Constants.BetfairConstants.INSTRUCTIONS] = instructions;
            args[Constants.BetfairConstants.CUSTOMER_REFERENCE] = customerRef;

            return await Invoke<ReplaceExecutionReport>(Constants.BetfairConstants.REPLACE_ORDERS_METHOD, args);
        }

        public async Task<UpdateExecutionReport> updateOrders(String marketId, IList<UpdateInstruction> instructions, String customerRef)
        {
            var args = new Dictionary<string, object>();
            args[Constants.BetfairConstants.MARKET_ID] = marketId;
            args[Constants.BetfairConstants.INSTRUCTIONS] = instructions;
            args[Constants.BetfairConstants.CUSTOMER_REFERENCE] = customerRef;

            return await Invoke<UpdateExecutionReport>(Constants.BetfairConstants.UPDATE_ORDERS_METHOD, args);
        }


        public async Task<IList<MarketTypeResult>> listMarketTypes(MarketFilter marketFilter, string stringLocale)
        {
            var args = new Dictionary<string, object>();
            args[Constants.BetfairConstants.FILTER] = marketFilter;
            args[Constants.BetfairConstants.LOCALE] = stringLocale;
            return await Invoke<List<MarketTypeResult>>(Constants.BetfairConstants.LIST_MARKET_TYPES_METHOD, args);

        }

        public async Task<T> Invoke<T>(string method, IDictionary<string, object> args)
        {
            // init result
            // T result = new T();

            if (method == null)
                throw new ArgumentNullException("method");
            if (method.Length == 0)
                throw new ArgumentException(null, "method");

            var restEndpoint = _betfairSettings?.BetfairEndpoints?.BettingEndpoint + "SportsAPING/v1.0/";

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, restEndpoint);

            var appKey = _betfairSettings?.BetfairLoginCredentials?.AppKey;
            if (appKey == null)
                throw new System.Exception("AppKey should not be null here. This exception should never be raised.");
            
            requestMessage.AddBaseHeaders(appKey);

            // if there is an auth token I add it, otherwhise no.
            if(AuthToken != null)
            {
                requestMessage.AddAuthHeader(AuthToken);
            }
            else
            {
                _logger.LogWarning("The authentication token is not configured.");
            }

            var postData = new StringContent(JsonSerializer.Serialize<IDictionary<string, object>>(args), Encoding.UTF8, "application/json");
            var call = new JsonRequest { Method = method, Id = 1, Params = args };

            var xd = JsonConvert.Serialize<JsonRequest>(call);
            var postData2 = new StringContent(xd, Encoding.UTF8, "application/json");

            // var postData = JsonConvert.Serialize<IDictionary<string, object>>(args) + "}";
            
            requestMessage.Content = postData2;

            _logger.LogDebug("Calling method <" + method + "> With args: " + postData2.ReadAsStringAsync().Result);

            HttpResponseMessage httpResponse = await HttpClientSingleton.Instance.Client.SendAsync(requestMessage);
            if(httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                try
                {
                    string httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                    return JsonConvert.Deserialize<T>(httpResponseBody);
                }
                catch (System.Exception e)
                {
                    _logger.LogError("Error while calling method <" + method + ">. Value returned is " + httpResponse.Content.ReadAsStringAsync().ToString());
                    throw new HttpRequestException(e.Message);
                }
            }
            else
            {
                string errorMessage = "Exception when calling method <" + method + ">. Response code <" + httpResponse.StatusCode + "> is not OK.";
                string httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                _logger.LogError(errorMessage);
                _logger.LogError(httpResponseBody);
                // throw ReconstituteException(JsonConvert.Deserialize<bf_bot.TO.Exception>(httpResponseBody));
                throw new BetfairClientException(_logger, errorMessage);
            }
        }


        public async Task<bool> RequestLogin()
        {
            _logger.LogInformation("Login requested.");
            var loginResult = await this.Login();
            if(loginResult.IsOk)
            {
                _logger.LogInformation("Successfully logged in.");
                _logger.LogInformation("Token: " + AuthToken);
                return true;
            }
            else
            {
                _logger.LogWarning("Cant authenticate user.");
                _logger.LogDebug(Utility.PrettyJsonObject(loginResult));
                return false;
            } 
        }
        private async Task<BetfairLoginResponse> Login()
        {
            _logger.LogInformation("Requested login to betfair.");
            BetfairLoginResponse result = new BetfairLoginResponse
            {
                IsOk = false
            };

            var bf_username = _betfairSettings?.BetfairLoginCredentials?.Username;
            var bf_password = _betfairSettings?.BetfairLoginCredentials?.Password;

            if (bf_username == null || bf_password == null)
            {
                _logger.LogError("Username or Password are null. This should never happen.");
                throw new System.Exception("Username or Password should not be null here. This exception should never happen.");
            }

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", bf_username),
                new KeyValuePair<string, string>("password", bf_password)
            });

            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, _betfairSettings?.BetfairEndpoints?.AuthEndpoint);

            var appKey = _betfairSettings?.BetfairLoginCredentials?.AppKey;
            if (appKey == null)
            {
                _logger.LogError("AppKey is null. This should never happen.");
                throw new System.Exception("AppKey should not be null here. This exception should never be raised.");
            }

            requestMessage.AddBaseHeaders(appKey);
            requestMessage.Content = content;

            HttpResponseMessage httpResponse = await HttpClientSingleton.Instance.Client.SendAsync(requestMessage);
            
            if(httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                string httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
                try
                {
                    var jres = JsonSerializer.Deserialize<BetfairLoginResponse>(httpResponseBody);
                    if (jres == null)
                        throw new System.Exception("Error while deserializing object.");
                    result = jres;

                    if(result.Status == "SUCCESS")
                    {
                        AuthToken = result.Token;
                        result.HttpResponseMessage = httpResponse;
                        result.IsOk = true;
                    }
                    else
                    {
                        result.IsOk = false;
                    }
                }
                catch (System.Exception e)
                {
                    _logger.LogError("An error has been encountered while reading the login response. Trace:\n" + e.Message);
                    Console.WriteLine(e);
                }
            }
            else
            {
                result.IsOk = false;
                result.HttpResponseMessage = httpResponse;
            }
            return result;
        }
    }
}