using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace bf_bot
{
    public static class Utility
    {
        public static string PrettyJsonObject(object o)
        {
            return JsonSerializer.Serialize(o, new JsonSerializerOptions{
                WriteIndented = true
            });
        }
    }
}