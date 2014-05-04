using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ClientEnterEvent : ClientEvent
    {
        public override ClientEventType Type { get { return ClientEventType.Enter; } }

        public ClientEnterEvent() { }
        public ClientEnterEvent(BinaryReader reader) { } 
        public override void Write(BinaryWriter writer) { }
    }
}
