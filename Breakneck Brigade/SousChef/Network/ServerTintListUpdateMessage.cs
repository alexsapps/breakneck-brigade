using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SousChef
{
    public class ServerTintListUpdateMessage : ServerMessage
    {
        public override ServerMessageType Type { get { return ServerMessageType.TintListUpdate; } }
        public List<string> TintList;
        public string Team;
        public ServerTintListUpdateMessage() { TintList = new List<string>(); }
        public override void Write(BinaryWriter writer)
        {
            writer.Write(Team);
            writer.Write((Int16)TintList.Count);
            foreach (var ing in TintList)
                writer.Write(ing);
        }
        public override void Read(BinaryReader reader)
        {
            this.Team = reader.ReadString();
            int count = reader.ReadInt16();
            for(int i = 0;  i < count; i++)
                TintList.Add(reader.ReadString());
        }
    }
}
