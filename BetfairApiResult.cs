using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bf_bot
{
    public class BetfairApiResult<T>
    {
        public bool IsSuccessfull { get; set; }
        public T? Details { get; set; }
        public HttpResponseMessage? HttpResponseMessage { get; set; }
    }
}