using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCuisine
{
    class ServerTeam
    {
        public List<Client> Members { get; private set; }
        public int Points { get; set; }
        public string Name { get; set; }

        public ServerTeam(string name)
        {
            Members = new List<Client>();
            this.Name = name;
        }
    }
}
