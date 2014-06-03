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
        public Dictionary<string, HashSet<string>> TintedObjects { get; protected set; }
        public List<IngredientType> Goals { get; protected set; }
        public int LookatId { get; set; }
        public int HeldId { get; set; }

        public ConfigSalad Config { get; private set; }
        public List<Recipe> Recipies { get; private set; }

        public ClientGame(BBLock @lock)
        {
            Lock = @lock;
            
            this.LiveGameObjects = new Dictionary<int, ClientGameObject>();
            this.GameObjectsCache = new Dictionary<int, ClientGameObject>();
            this.Goals = new List<IngredientType>();
            this.Config = new GameObjectConfig().GetConfigSalad();
            this.Recipies = this.Config.Recipes.Values.ToList();
            this.TintedObjects = new Dictionary<string, HashSet<string>>();
            this.TintedObjects.Add("red", new HashSet<string>());
            this.TintedObjects.Add("blue", new HashSet<string>());
            this.LookatId = -1;
            this.HeldId = -1;
        }


    }
}
