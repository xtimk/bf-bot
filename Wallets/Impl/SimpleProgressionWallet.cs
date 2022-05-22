using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace bf_bot.Wallets.Impl
{
    public class SimpleProgressionWallet : IWallet
    {
        private double _balance;
        
        // trace step just for printing purposes, it is really not necessary for the logic.
        private int _step;
        private readonly double _win_per_cycle;
        private double _desired_wallet_balance;

        public SimpleProgressionWallet(double balance, double win_per_cycle)
        {
            _win_per_cycle = win_per_cycle;
            _balance = balance;
            _desired_wallet_balance = _balance + _win_per_cycle;
        }

        public double getAmountToBet(double price)
        {
            var amountToWin = _desired_wallet_balance - _balance;
            return amountToWin/(price - 1);
        }

        public void signalPlaceBet(double amount)
        {
            _balance -= amount;
        }

        public void signalWin(double amount)
        {
            _balance += amount;
            _desired_wallet_balance = _balance + _win_per_cycle;
            _step = 1;
        }

        public double getBalance()
        {
            return _balance;
        }
    }
}