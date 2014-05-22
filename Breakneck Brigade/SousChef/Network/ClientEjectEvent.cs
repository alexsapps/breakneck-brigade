using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SousChef
{
    public class ClientEjectEvent : ClientEvent
    {
        public override ClientEventType Type { get { return ClientEventType.Eject; } }

        public ClientEjectEvent() { }

        public ClientEjectEvent(BinaryReader reader)
        {
        }

        public override void Write(BinaryWriter writer)
        {
        }
    }
}
