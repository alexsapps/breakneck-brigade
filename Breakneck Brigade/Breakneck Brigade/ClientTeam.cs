using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breakneck_Brigade
{
    class ClientTeam
    {
        public string Name { get; set; }
        public int Score { get; set; }
        public List<string> Clients { get; set; }

        public ClientTeam()
        {
            Clients = new List<string>();
        }
    }
}
