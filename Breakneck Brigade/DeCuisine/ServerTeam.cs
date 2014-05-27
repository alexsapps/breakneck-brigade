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
        public List<string> TintList{ get; set; }

        public ServerTeam(string name)
        {
            this.Members = new List<Client>();
            this.Name = name;
            this.TintList = new List<string>();
        }
    }
}
