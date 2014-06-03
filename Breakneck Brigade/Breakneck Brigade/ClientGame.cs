using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breakneck_Brigade.Graphics;

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
        public Dictionary<string, HashSet<string>> TintedObjects { get; protected set; }
        public List<IngredientType> Goals { get; protected set; }
        public int LookatId { get; set; }
        public int HeldId { get; set; }
        public List<AParticleSpawner> ParticleSpawners { get; set;}

        public ConfigSalad Config { get; private set; }

        public ClientGame(BBLock @lock)
        {
            Lock = @lock;
            
            LiveGameObjects = new Dictionary<int, ClientGameObject>();
            GameObjectsCache = new Dictionary<int, ClientGameObject>();
            ParticleSpawners = new List<AParticleSpawner>();
            Goals = new List<IngredientType>();

            Config = new GameObjectConfig().GetConfigSalad();
            TintedObjects = new Dictionary<string, HashSet<string>>();
            TintedObjects.Add("red", new HashSet<string>());
            TintedObjects.Add("blue", new HashSet<string>());
            LookatId = -1;
            HeldId = -1;
        }


    }
}
