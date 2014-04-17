using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCuisine
{
    class ServerGameStateUpdateMessage : ServerMessage
    {

        public Dictionary<int,ServerGameObject> GameObjects { get; set; }
    }
}
