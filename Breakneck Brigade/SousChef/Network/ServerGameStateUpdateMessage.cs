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
            BBStopwatch w1 = new BBStopwatch();
            w1.Start();
            writer.Write(Binary.Length);
            writer.Write(Binary);
            w1.Stop(2, "Client: slow game state write. {0}");
        }
        public override void Read(BinaryReader reader)
        {
            int len = reader.ReadInt32();
            Binary = reader.ReadBytes(len);
        }
    }
}
