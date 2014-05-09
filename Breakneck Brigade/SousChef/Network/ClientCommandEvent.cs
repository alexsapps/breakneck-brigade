using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ClientCommandEvent : ClientEvent
    {
        public override ClientEventType Type { get { return ClientEventType.Command; } }
        public string[] args;

        public ClientCommandEvent()
        {

        }

        public ClientCommandEvent(BinaryReader reader)
        {
            args = new string[reader.ReadInt32()];
            for (int i = 0; i < args.Length; i++)
                args[i] = reader.ReadString();
        }
        public override void Write(BinaryWriter writer)
        {
            writer.Write(args.Length);
            foreach(var arg in args)
            {
                writer.Write(arg);
            }
        }
    }
}
