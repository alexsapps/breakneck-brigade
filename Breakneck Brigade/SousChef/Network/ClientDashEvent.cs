using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SousChef
{
    public class ClientDashEvent : ClientEvent
    {
        public override ClientEventType Type { get { return ClientEventType.Dash; } }
        public bool isDashing { get; set; }

        public ClientDashEvent() { }
        public ClientDashEvent(BinaryReader reader)
        {
            this.isDashing = reader.ReadBoolean();
        }
        public override void Write(BinaryWriter writer)
        {
            writer.Write(isDashing);
        }
    }
}
