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
        public BBLock Lock;

        public int? PlayerObjId { get; set; }

        public TimeSpan GameTime { get; set; }

        //locking directly on gameObjects before accessing
        public Dictionary<int, ClientGameObject> LiveGameObjects { get; set; }
        public Dictionary<int, ClientGameObject> GameObjectsCache { get; set; }

        public ConfigSalad Config { get; private set; }

        public ClientGame(BBLock @lock)
        {
            Lock = @lock;
            
            LiveGameObjects = new Dictionary<int, ClientGameObject>();
            GameObjectsCache = new Dictionary<int, ClientGameObject>();

            Config = new GameObjectConfig().GetConfigSalad();
        }


    }
}
