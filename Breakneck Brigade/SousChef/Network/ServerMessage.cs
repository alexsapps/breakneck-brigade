using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SousChef;

namespace SousChef
{
    public abstract class ServerMessage
    {
        public DateTime Created;
        public abstract ServerMessageType Type { get; }

        public abstract void Write(System.IO.BinaryWriter writer);
    }
}
