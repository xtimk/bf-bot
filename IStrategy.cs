using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bf_bot
{
    public interface IStrategy
    {
        Task Start();
    }
}