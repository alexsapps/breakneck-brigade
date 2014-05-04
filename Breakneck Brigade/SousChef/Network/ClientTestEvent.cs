using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ClientTestEvent : ClientEvent
    {
        public override ClientEventType Type { get { return ClientEventType.Test; } }
        public ClientTestEvent() { }
        public ClientTestEvent(BinaryReader reader) { }
        public override void Write(BinaryWriter writer) { }
    }
}
