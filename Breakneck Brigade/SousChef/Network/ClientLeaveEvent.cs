using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ClientLeaveEvent : ClientEvent
    {
        public override ClientEventType Type { get { return ClientEventType.Leave; } }

        public ClientLeaveEvent() { }
        public ClientLeaveEvent(BinaryReader reader) { } 
        public override void Write(BinaryWriter writer) { }
    }
}
