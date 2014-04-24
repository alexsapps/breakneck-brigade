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

        //gameObjects and HasUpdates both require locking on gameObjects before accessing
        public Dictionary<int, ClientGameObject> gameObjects { get; private set; }
        public ConfigSalad Config { get; private set; }
        public bool HasUpdates { get; set; } //indicates that data has been received from the network or the game has ended.

        public ClientGame()
        {
            gameObjects = new Dictionary<int, ClientGameObject>();
            Config = new GameObjectConfig().GetConfigSalad();
        }


        
    }
}
