using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ServerPlayerIdUpdateMessage : ServerMessage
    {
        public int PlayerId { get; set; }
        public override ServerMessageType Type { get { return ServerMessageType.PlayerIdUpdate; } }

        public override void Write(BinaryWriter writer)
        {
            writer.Write((Int32)PlayerId);
        }
        public override void Read(BinaryReader reader)
        {
            PlayerId = reader.ReadInt32();
        }
    }
}
