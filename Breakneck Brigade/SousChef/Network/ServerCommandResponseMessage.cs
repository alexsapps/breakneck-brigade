using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ServerCommandResponseMessage : ServerMessage
    {
        public string Result { get; set; }
        public override ServerMessageType Type { get { return ServerMessageType.ServerCommandResponse; } }

        public ServerCommandResponseMessage() { }
        public override void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(Result);
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            Result = reader.ReadString();
        }
    }
}
