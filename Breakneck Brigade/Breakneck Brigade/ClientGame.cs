using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breakneck_Brigade
{
    class ClientGame
    {
        public BBLock Lock = new BBLock();

        public int PlayerObjId { get; set; }

        //locking directly on gameObjects before accessing
        public Dictionary<int, ClientGameObject> gameObjects { get; private set; }
        public ConfigSalad Config { get; private set; }

        public ClientGame()
        {
            gameObjects = new Dictionary<int, ClientGameObject>();
            Config = new GameObjectConfig().GetConfigSalad();
        }
   
    }
}
