using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SousChef
{
    public class ClientHintEvent : ClientEvent
    {
        public override ClientEventType Type { get { return ClientEventType.Hint; } }
        public bool isJumping { get; set; }

        public ClientHintEvent() { }
        public ClientHintEvent(BinaryReader reader)
        {
        }
        public override void Write(BinaryWriter writer)
        {
        }
    }
}
