using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class Cooker : GameObject
    {
        public CookerType Type { get; set; }
        public List<Ingredient> Contents { get; private set; }

        public Cooker (int id, CookerType type)
            : base(id)
        {
            this.Type = type;
            Contents = new List<Ingredient>();
        }

        public Recipe Cook()
        {
            Contents.Sort();
            
            string hash = Recipe.Hash(Contents.ConvertAll<IngredientType>(x => x.Type));

            foreach (var recipe in Type.Recipes)
            {
                if (recipe.hash() == hash)
                {
                    //TODO: clear contents and put the actual ingredient here (and still return the recipe for purpose of awarding points)
                    //      current problem:  how to initialize new game object--need id getter, need to keep track of new objects
                    return recipe;
                }
            }

            return null;
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
