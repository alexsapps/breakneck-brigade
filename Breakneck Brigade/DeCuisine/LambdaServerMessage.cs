using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCuisine
{
    delegate void ServerMessageWriter(System.IO.BinaryWriter writer);
    class LambdaServerMessage : ServerMessage
    {
        ServerMessageWriter serverMessageWriter;

        public LambdaServerMessage(ServerMessageWriter serverMessageWriter)
        {
            this.serverMessageWriter = serverMessageWriter;
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            serverMessageWriter(writer);
        }
    }
}
