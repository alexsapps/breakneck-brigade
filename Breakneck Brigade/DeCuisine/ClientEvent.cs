using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SousChef;

namespace DeCuisine
{
    class ClientEvent
    {
        public Client Client { get; set; }
        public ClientEventType Type { get; set; }
        public Dictionary<string, string> Args { get; set; }
    }
}
