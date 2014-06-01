using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ServerGoalsUpdateMessage : ServerMessage
    {
        public List<string> Goals { get; set; }

        public ServerGoalsUpdateMessage()
        {
            Goals = new List<string>();
        }

        public override ServerMessageType Type
        {
            get { return ServerMessageType.GoalsUpdate; }
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(Goals.Count);
            foreach(var goal in Goals) {
                writer.Write(goal);
            }
        }

        public override void Read(System.IO.BinaryReader reader)
        {
            int len = reader.ReadInt32();
            for(int i = 0; i < len; i++) {
                Goals.Add(reader.ReadString());
            }
        }
    }
}
