using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SousChef;

namespace DeCuisine
{
    public abstract class ServerMessage
    {
        public DateTime Created;
        public ServerMessageType Type { get; set; }

        public abstract void Write(System.IO.BinaryWriter writer);
    }
}
