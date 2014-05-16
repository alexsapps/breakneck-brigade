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
        public float Orientation { get; set; }
        public float Incline { get; set; }
        public string Hand { get; set; }

        public ClientThrowEvent() { }
        public ClientThrowEvent(BinaryReader reader) 
        {
            this.Hand = reader.ReadString();
            this.Orientation = reader.ReadSingle();
            this.Incline = reader.ReadSingle();
        }
        public override void Write(BinaryWriter writer) 
        {
            writer.Write(this.Hand);
            writer.Write(this.Orientation);
            writer.Write(this.Incline);
        }
    }
}
