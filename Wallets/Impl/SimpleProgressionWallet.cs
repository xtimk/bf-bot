using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using bf_bot.Elasticsearch;
using bf_bot.Utils;
using Microsoft.Extensions.Logging;
using Nest;

namespace bf_bot.Wallets.Impl
{
    public class SimpleProgressionWallet : IWallet
    {
        private double _balance;
        // trace step just for printing purposes, it is really not necessary for the logic.
        private int _step;
        private double _win_per_cycle;
        private double _desired_wallet_balance;
        private double _lastBetAmount;
        private double _lastPriceAmount;
        private string _wallet_type_name = "Simple Progression Wallet";
        private readonly ILogger _logger;
        private ElasticClient _esClient;
        private string _betDescription;
        private readonly AppGuid _sessionGuid;

        private void IndexDocument<T>(T doc) where T : GenericDocument
        {
            doc.Timestamp = DateTime.Now;
            doc.DocType = typeof(T).ToString();
            doc.SessionGuid = _sessionGuid.AppSessionId.ToString();

            var indexResponse = _esClient.IndexDocument(doc);
            if (!indexResponse.IsValid)
            {
                _logger.LogError("Error when indexing app data into elastic. Details: " + indexResponse.OriginalException );
            }
        }

        public SimpleProgressionWallet(ILogger<SimpleProgressionWallet> logger, AppGuid sessionGuid)
        {
            _logger = logger;
            _sessionGuid = sessionGuid;
        }
        public bool Init(double balance, double win_per_cycle, ElasticClient esClient)
        {
            _esClient = esClient;
            _win_per_cycle = win_per_cycle;
            _balance = balance;
            _desired_wallet_balance = _balance + _win_per_cycle;
            _step = 1;
            _betDescription = "";

            var walletEsDoc = new InitDocument(){
                Balance = _balance,
                Message = "Bot started",
            };
            IndexDocument(walletEsDoc);

            return true;
        }

        public double getAmountToBet(double price)
        {
            var amountToWin = _desired_wallet_balance - _balance;
            return amountToWin/(price - 1);
        }

        public void signalPlaceBet(double amount, double price, string betDescription)
        {
            _balance -= amount;
            _lastBetAmount = amount;
            _lastPriceAmount = price;
            _betDescription = betDescription;
            _logger.LogInformation("Step " + _step + " - Bet placed: " + amount + "@" + price);
            _logger.LogInformation("Step " + _step + " - Current balance is " + _balance + "EUR. The desired balance after cycle is: " + _desired_wallet_balance + "EUR.");
            _logger.LogInformation("Step " + _step + " - Actual balance if bet wins (end the cycle) at this step will be " + (_balance + (amount * price)) + "EUR");

            var walletEsDoc = new SignalBetDocument(){
                Step = _step,
                Balance = _balance,
                BetAmount = amount,
                Price = price,
                Message = "Bet placed: " + amount + "@" + price,
                BetfairLink = _betDescription,
            };
            IndexDocument(walletEsDoc);
        }

        public void signalWin(double amount)
        {
            _balance += amount;
            _desired_wallet_balance = _balance + _win_per_cycle;

            _logger.LogInformation("Cycle closed at step " + _step + ". Balance is " + _balance + "EUR. Desired balance after cycle is: " + _desired_wallet_balance);

             var walletEsDoc = new SignalWinDocument(){
                Step = _step,
                Balance = _balance,
                Message = "Bet winned",
                BetfairLink = _betDescription,
            };
            IndexDocument(walletEsDoc);

            _step = 1;
            _betDescription = "";
        }

        public void signalWin()
        {
            _balance += (_lastBetAmount * _lastPriceAmount);
            _desired_wallet_balance = _balance + _win_per_cycle;
            
            _logger.LogInformation("Cycle closed at step " + _step + ". Balance is " + _balance + "EUR. Desired balance after next cycle is: " + _desired_wallet_balance);
            
            var walletEsDoc = new SignalWinDocument(){
                Step = _step,
                Balance = _balance,
                Message = "Bet winned",
                BetfairLink = _betDescription,
            };
            IndexDocument(walletEsDoc);

            _step = 1;
            _betDescription = "";
        }

        public void signalLose()
        {
            _logger.LogInformation("Step " + _step + " - Bet lost. Will now increment step. Balance is " + _balance + "EUR. Desired balance after cycle is: " + _desired_wallet_balance);

            var walletEsDoc = new SignalLoseDocument(){
                Step = _step,
                Balance = _balance,
                Message = "Bet lost",
                BetfairLink = _betDescription,
            };
            IndexDocument(walletEsDoc);         
            
            _step++;
            _betDescription = "";
        }

        public double getBalance()
        {
            return _balance;
        }

        public string getWalletName()
        {
            return _wallet_type_name;
        }
    }
}