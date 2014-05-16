using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCuisine
{
    class ServerTeam
    {
        public List<ServerPlayer> Players { get; private set; }
        public int Points { get; set; }

        public ServerTeam()
        {
            Players = new List<ServerPlayer>();
        }
    }
}
