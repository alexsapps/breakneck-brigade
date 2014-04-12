using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ServerGameModeUpdateMessage : ServerMessage
    {
        public GameMode Mode { get; set; }
    }
}
