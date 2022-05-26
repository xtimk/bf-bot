using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace bf_bot.Wallets
{
    public interface IWallet
    {

                /// <summary>
        /// Given the price returns the amount to bet
        /// </summary>
        void Init(double balance, double win_per_cycle);
        /// <summary>
        /// Given the price returns the amount to bet
        /// </summary>
        double getAmountToBet(double price);

        /// <summary>
        /// Gets the actual balance of the wallet
        /// </summary>
        double getBalance();

        /// <summary>
        /// Signals the wallet that a bet has been placed
        /// </summary>
        void signalPlaceBet(double amount, double price);

        /// <summary>
        /// Signals the wallet that a bet was won
        /// </summary>
        void signalWin(double amount);
        
        /// <summary>
        /// Signals the wallet that a bet was won
        /// </summary>
        void signalWin();

        /// <summary>
        /// Signals the wallet that a bet is lost
        /// </summary>
        void signalLose();
        
        /// <summary>
        /// Gets the wallet name
        /// </summary>
        string getWalletName();

    }
}