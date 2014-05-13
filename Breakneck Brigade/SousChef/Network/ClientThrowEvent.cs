using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SousChef
{
    public class ClientThrowEvent : ClientEvent
    {
        public override ClientEventType Type { get { return ClientEventType.ThrowItem; } }
        public Coordinate Impulse { get; set; }
        public string Hand { get; set; }

        public ClientThrowEvent() { }
        public ClientThrowEvent(BinaryReader reader) 
        {
            this.Hand = reader.ReadString();
            this.Impulse = new Coordinate(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }
        public override void Write(BinaryWriter writer) 
        {
            writer.Write(this.Hand);
            writer.Write(this.Impulse.x);
            writer.Write(this.Impulse.y);
            writer.Write(this.Impulse.z);
        }
    }
}
