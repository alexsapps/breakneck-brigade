using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SousChef;

namespace SousChef
{
    public class ClientEvent
    {
        public ClientEventType Type { get; set; }
        public Dictionary<string, string> Args { get; set; }
    }
}
