using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bf_bot
{
    public class BetfairApiResult<T>
    {
        // true if request succeeded
        // false if request failed (application failure)
        // null if the request fails with code != 200
        public bool IsRequestOk { get; set; }
        public bool IsSuccessfull { get; set; }
        public T? Details { get; set; }
        public HttpResponseMessage? HttpResponseMessage { get; set; }
    }
}