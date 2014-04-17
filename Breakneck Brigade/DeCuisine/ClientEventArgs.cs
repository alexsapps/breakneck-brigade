using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCuisine
{
    public class ClientEventArgs
    {
        public Client Client { get; set; }
        public ClientEventArgs(Client client) { this.Client = client; }
    }
}
