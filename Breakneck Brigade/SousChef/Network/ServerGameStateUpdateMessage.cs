using SousChef;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ServerGameStateUpdateMessage : ServerMessage
    {
        public override ServerMessageType Type { get { return ServerMessageType.GameStateUpdate; } }
        public byte[] Binary;
        public ServerGameStateUpdateMessage() { }
        public override void Write(BinaryWriter writer)
        {
            writer.Write(Binary.Length);
            writer.Write(Binary);
        }
        public override void Read(BinaryReader reader)
        {
            int len = reader.ReadInt32();
            Binary = reader.ReadBytes(len);
        }
    }
}
