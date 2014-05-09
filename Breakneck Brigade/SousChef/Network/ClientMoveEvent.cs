using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ClientBeginMoveEvent : ClientEvent
    {
        public override ClientEventType Type { get { return ClientEventType.BeginMove; } }
        public Coordinate Delta { get; set; }

        public ClientBeginMoveEvent() { }
        public ClientBeginMoveEvent(BinaryReader reader) 
        {
            Delta = new Coordinate(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
        public override void Write(BinaryWriter writer) 
        {
            writer.Write(Delta.x);
            writer.Write(Delta.y);
            writer.Write(Delta.z);
        }
    }
}
