using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bf_bot
{
    public class BetfairClientInitializer
    {
        public BetfairEndpoints? BetfairEndpoints { get; set; }
        public BetfairLoginCredentials? BetfairLoginCredentials { get; set; }
    }
}