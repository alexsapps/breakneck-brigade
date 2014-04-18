using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

namespace DeCuisine
{
    /// <summary>
    /// The server cooker object
    /// </summary>
    class ServerCooker : ServerGameObject
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Cooker; } }
        public Dictionary<string, Recipe> Recipes { get; set; }
        public Dictionary<string, bool> ValidIngredients { get; set; }
        public List<ServerIngredient> Contents { get; private set; }
        public CookerType Type { get; set; }

        private string HashCache { get; set; }
        
        /// <summary>
        /// Makes a servercooker object on the server
        /// </summary>
        /// <param name="id">Unique id on the server. Should be given to the client to make
        /// a ClientCooker with the same id</param>
        /// <param name="transform">Initial location</param>
        /// <param name="server">The server where the cooker is made</param>
        /// <param name="type">What type of cooker i.e "oven"</param>
        public ServerCooker(int id, CookerType type, ServerGame game)
            : base(id, game)
        {
            this.Type = type;
            Contents = new List<ServerIngredient>();
        }

        /*
         * Adds the ingredient to the list. Keeps the list in sorted order. If the 
         * ingredient to add isn't in the valid ingredient table, don't add and return false
         */
        public bool AddIngredient(ServerIngredient ingredient)
        {
            HashCache = null;
            if (Type.ValidIngredients.ContainsKey(ingredient.Type.Name))
            {
                Contents.Add(ingredient);
                return true;
            }
            return false;
        }

        /*
         * Should loop over the contents, calculate the score, attatch the score to the 
         * return object and return a final product with the attatched score. 
         * TODO: Make it do that^
         */
        public IngredientType Cook()
        {
            if (HashCache == null)
            {
                //recompute hash since an item was added since last cook
                Contents = Contents.OrderBy(o => o.Type.Name).ToList();//put in sorted order before hashing
                this.HashCache = Recipe.Hash(Contents.ConvertAll<IngredientType>(x => x.Type));
            }

            if (Type.Recipes.ContainsKey(this.HashCache))
            {
                return Type.Recipes[this.HashCache].FinalProduct;
            }
            return null;
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
