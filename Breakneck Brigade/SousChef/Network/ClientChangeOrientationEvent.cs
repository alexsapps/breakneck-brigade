using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ClientChangeOrientationEvent : ClientEvent
    {
        public override ClientEventType Type { get { return ClientEventType.ChangeOrientation; } }
        public float Orientation { get; set; }
        public float Incline { get; set; }

        public ClientChangeOrientationEvent() { }
        public ClientChangeOrientationEvent(BinaryReader reader) 
        {
            Orientation = reader.ReadSingle();
            Incline = reader.ReadSingle();
        } 
        public override void Write(BinaryWriter writer) 
        {
            writer.Write(Orientation);
            writer.Write(Incline);
        }
    }
}
