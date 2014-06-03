using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ServerSoundMessage : ServerMessage
    {
        public Vector4 Location { get; set; }
        public BBSound Sound { get; set; }
        
        public override ServerMessageType Type
        {
            get { return ServerMessageType.Sound; }
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(Location);
            writer.Write((Int32)Sound);
        }

        public override void Read(System.IO.BinaryReader reader)
        {
            Location = reader.ReadCoordinate();
            Sound = (BBSound)reader.ReadInt32();
        }
    }
}
