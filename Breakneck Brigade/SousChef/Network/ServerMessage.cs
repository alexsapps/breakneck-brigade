using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SousChef;
using System.IO;

namespace SousChef
{
    public abstract class ServerMessage
    {
        public DateTime Created;
        public abstract ServerMessageType Type { get; }

        public abstract void Write(BinaryWriter writer);
        public abstract void Read(BinaryReader reader); //using constructor causes exception handling in caller to be ignored when using Activator.CreateInstance to call constructor and read data.
    }
}
