using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bf_bot.Utils
{
    public class AppGuid
    {
        public Guid AppSessionId { get; set; }
        public AppGuid()
        {
            AppSessionId = Guid.NewGuid();
        }
    }
}