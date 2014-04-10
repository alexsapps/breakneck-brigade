using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ServerGameStateUpdateMessage : ServerMessage
    {

        public Dictionary<int,GameObject> GameObjects { get; set; }
    }
}
