using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ServerGameStateUpdateMessage : ServerMessage
    {

        public Dictionary<string,string> GameObjects { get; set; }
    }
}
