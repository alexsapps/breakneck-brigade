using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ServerGoalsUpdateMessage : BinaryServerMessage
    {
        public ServerGoalsUpdateMessage()
        {
            
        }

        public override ServerMessageType Type
        {
            get { return ServerMessageType.GoalsUpdate; }
        }
    }
}
