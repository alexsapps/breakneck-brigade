using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class EasyClientEvent : ClientEvent
    {
        public Dictionary<string, string> Args { get; set; }
        public EasyClientEvent()
        {
            Args = new Dictionary<string, string>();
        }
    }
}
