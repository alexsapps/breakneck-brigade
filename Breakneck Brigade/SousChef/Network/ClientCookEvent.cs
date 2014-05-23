using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SousChef;
namespace SousChef
{
    public class ClientCooktEvent : ClientEvent
    {
        public override ClientEventType Type { get { return ClientEventType.Eject; } }

        public ClientCookEvent() { }

        public ClientCookEvent(BinaryReader reader)
        {
        }

        public override void Write(BinaryWriter writer)
        {
        }
    }
}