using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class Recipe 
    {
        public List<Ingredient> Ingredients;
        public string Cooker; 
        public Ingredient FinalProduct;
        
        public Recipe(List<Ingredient> ingredients, string cooker, Ingredient finalProduct, string modelLoc)
        {
            this.Ingredients = new List<Ingredient>();
            foreach(var ingredient in ingredients)
            {
                this.Ingredients.Add(ingredient);
            }
            this.Cooker = cooker;
            this.FinalProduct = finalProduct;
        }
    }
}
