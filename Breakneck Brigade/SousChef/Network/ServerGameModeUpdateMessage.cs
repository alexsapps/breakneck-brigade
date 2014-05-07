using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using System.IO;

namespace SousChef
{
    public class ServerGameModeUpdateMessage : ServerMessage
    {
        public GameMode Mode { get; set; }
        public override ServerMessageType Type { get { return ServerMessageType.GameModeUpdate; } }
        public override void Write(BinaryWriter writer)
        {
            writer.Write((byte)(Mode));
        }

        public ServerGameModeUpdateMessage() { }
        public ServerGameModeUpdateMessage(BinaryReader reader)
        {
            Mode = (GameMode)reader.ReadByte();
        }
    }
}
