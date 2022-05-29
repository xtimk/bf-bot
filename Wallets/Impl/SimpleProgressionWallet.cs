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
        private double _lastPriceAmount;
        private string _wallet_type_name = "Simple Progression Wallet";
        private readonly ILogger _logger;

        // public SimpleProgressionWallet(double balance, double win_per_cycle)
        // {
        //     _win_per_cycle = win_per_cycle;
        //     _balance = balance;
        //     _desired_wallet_balance = _balance + _win_per_cycle;
        // }
        public SimpleProgressionWallet(ILogger<SimpleProgressionWallet> logger)
        {
            _logger = logger;
        }
        public bool Init(double balance, double win_per_cycle)
        {
            _win_per_cycle = win_per_cycle;
            _balance = balance;
            _desired_wallet_balance = _balance + _win_per_cycle;
            _step = 1;
            return true;
        }

        public double getAmountToBet(double price)
        {
            var amountToWin = _desired_wallet_balance - _balance;
            return amountToWin/(price - 1);
        }

        public void signalPlaceBet(double amount, double price)
        {
            _balance -= amount;
            _lastBetAmount = amount;
            _lastPriceAmount = price;
            _logger.LogInformation("Step " + _step + " - Bet placed: " + amount + "@" + price);
            _logger.LogInformation("Step " + _step + " - Current balance is " + _balance + "EUR. The desired balance after cycle is: " + _desired_wallet_balance + "EUR.");
            _logger.LogInformation("Step " + _step + " - Actual balance if bet wins (end the cycle) at this step will be " + (_balance + (amount * price)) + "EUR");
        }

        public void signalWin(double amount)
        {
            _balance += amount;
            _desired_wallet_balance = _balance + _win_per_cycle;
            _logger.LogInformation("Cycle closed at step " + _step + ". Balance is " + _balance + "EUR. Desired balance after cycle is: " + _desired_wallet_balance);
            _step = 1;
        }

        public void signalWin()
        {
            _balance += (_lastBetAmount * _lastPriceAmount);
            _desired_wallet_balance = _balance + _win_per_cycle;
            _logger.LogInformation("Cycle closed at step " + _step + ". Balance is " + _balance + "EUR. Desired balance after cycle is: " + _desired_wallet_balance);
            _step = 1;
        }

        public void signalLose()
        {
            _logger.LogInformation("Step " + _step + " - Bet lost. Will now increment step. Balance is " + _balance + "EUR. Desired balance after cycle is: " + _desired_wallet_balance);
            _step++;
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