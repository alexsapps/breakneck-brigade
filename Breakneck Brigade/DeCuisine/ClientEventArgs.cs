using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCuisine
{
    class ClientEventArgs
    {
        public Client Client { get; set; }
        public ClientEventArgs(Client client) { this.Client = client; }
    }
}
