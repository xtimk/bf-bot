using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

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
        private string _wallet_type_name = "Simple Progression Wallet";
        private readonly ILogger<SimpleProgressionWallet> _logger;

        // public SimpleProgressionWallet(double balance, double win_per_cycle)
        // {
        //     _win_per_cycle = win_per_cycle;
        //     _balance = balance;
        //     _desired_wallet_balance = _balance + _win_per_cycle;
        // }
        public SimpleProgressionWallet(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SimpleProgressionWallet>();
        }
        public void Init(double balance, double win_per_cycle)
        {
            _win_per_cycle = win_per_cycle;
            _balance = balance;
            _desired_wallet_balance = _balance + _win_per_cycle;
            _step = 1;
        }

        public double getAmountToBet(double price)
        {
            var amountToWin = _desired_wallet_balance - _balance;
            return amountToWin/(price - 1);
        }

        public void signalPlaceBet(double amount)
        {
            _balance -= amount;
            _lastBetAmount = amount;
            _logger.LogInformation("Balance is " + _balance + "EUR. Desired balance after cycle is: " + _desired_wallet_balance);
            _logger.LogInformation("Current step is: " + _step);
        }

        public void signalWin(double amount)
        {
            _balance += amount;
            _desired_wallet_balance = _balance + _win_per_cycle;
            _step = 1;
            _logger.LogInformation("Balance is " + _balance + "EUR. Desired balance after cycle is: " + _desired_wallet_balance);
        }

        public void signalWin()
        {
            _balance += _lastBetAmount;
            _desired_wallet_balance = _balance + _win_per_cycle;
            _step = 1;
            _logger.LogInformation("Balance is " + _balance + "EUR. Desired balance after cycle is: " + _desired_wallet_balance);
        }

        public void signalLose()
        {
            _step++;
            _logger.LogInformation("Balance is " + _balance + "EUR. Desired balance after cycle is: " + _desired_wallet_balance);
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