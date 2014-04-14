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
        private string ContentsHash { get; set; }

        public Cooker (int id, CookerType type)
            : base(id)
        {
            this.Type = type;
            Contents = new List<Ingredient>();
        }


        /*
         * Adds the ingredient to the list. Keeps the list in sorted order. If the 
         * ingredient to add isn't in the valid ingredient table, don't add and return false
         */
        public bool AddIng(Ingredient ingToAdd)
        {
            if (Type.ValidIngredients.ContainsKey(ingToAdd.Type.Name))
            {
                Contents.Add(ingToAdd);
                Contents = Contents.OrderBy(o => o.Type.Name).ToList();//should keep in sorted order
                this.ContentsHash = Recipe.Hash(Contents.ConvertAll<IngredientType>(x => x.Type));
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
            if (Type.Recipes.ContainsKey(this.ContentsHash))
            {
                return Type.Recipes[this.ContentsHash].FinalProduct;
            }
            return null;
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
