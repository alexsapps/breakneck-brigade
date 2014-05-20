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
        public string Name;

        public ServerTeam(string name)
        {
            Players = new List<ServerPlayer>();
            this.Name = name;
        }
    }
}
