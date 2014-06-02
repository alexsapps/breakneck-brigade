using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SousChef
{
    public class ClientLeftClickEvent : ClientEvent
    {
        public override ClientEventType Type { get { return ClientEventType.LeftClickEvent; } }
        public float Orientation { get; set; }
        public float Incline { get; set; }
        public string Hand { get; set; }
        public float Force { get; set; }

        public ClientLeftClickEvent() { }
        public ClientLeftClickEvent(BinaryReader reader) 
        {
            this.Hand = reader.ReadString();
            this.Orientation = reader.ReadSingle();
            this.Incline = reader.ReadSingle();
            this.Force = reader.ReadSingle();
        }
        public override void Write(BinaryWriter writer) 
        {
            writer.Write(this.Hand);
            writer.Write(this.Orientation);
            writer.Write(this.Incline);
            writer.Write(this.Force);
        }
    }
}
