using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

namespace DeCuisine
{
    public class ServerCooker : ServerGameObject
    {
        public List<ServerIngredient> Contents { get; private set; }
        private string HashCache { get; set; }

        public ServerCooker (int id, Vector4 transform, Server server, CookerType type)
            : base(id)
        {
            this.Type = type;
            Contents = new List<Ingredient>();
        }

        /*
         * Adds the ingredient to the list. Keeps the list in sorted order. If the 
         * ingredient to add isn't in the valid ingredient table, don't add and return false
         */
        public bool AddIngredient(Ingredient ingredient)
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
