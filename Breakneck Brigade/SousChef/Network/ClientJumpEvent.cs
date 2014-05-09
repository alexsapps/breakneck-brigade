using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SousChef
{
    public class ClientJumpEvent : ClientEvent
    {
        public override ClientEventType Type { get { return ClientEventType.Jump; } }
        public bool isJumping { get; set; }

        public ClientJumpEvent() { }
        public ClientJumpEvent(BinaryReader reader)
        {
            this.isJumping = reader.ReadBoolean();
        }
        public override void Write(BinaryWriter writer)
        {
            writer.Write(isJumping);
        }
    }
}
