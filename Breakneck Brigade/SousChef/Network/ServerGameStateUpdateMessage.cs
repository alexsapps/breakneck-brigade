using SousChef;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ServerGameStateUpdateMessage : BinaryServerMessage
    {
        public override ServerMessageType Type { get { return ServerMessageType.GameStateUpdate; } }
    }
}
