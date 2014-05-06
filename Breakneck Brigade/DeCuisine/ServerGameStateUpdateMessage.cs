using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCuisine
{
    class ServerGameStateUpdateMessage : ServerMessage
    {
        //public List<int> DeletedGameObjects { get; set; }
        //public List<ServerGameObject> ChangedGameObjects { get; set; }
        //public List<ServerGameObject> NewGameObjects { get; set; }

        public byte[] Binary;
        public int Length;

        public override void Write(System.IO.BinaryWriter writer)
        {
            writer.Write(Binary, 0, Length);                        
        }
    }
}
