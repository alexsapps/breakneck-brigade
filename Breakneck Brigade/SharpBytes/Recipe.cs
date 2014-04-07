using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpBytes
{
    class Recipe 
    {
        public List<string> ingredients;
        public string cooker; 
        public string finalProduct;
        
        public Recipe(List<string> ingredients, string cooker, string finalProduct, string modelLoc)
        {
            this.ingredients = new List<string>();
            foreach(string ingredient in ingredients)
            {
                this.ingredients.Add(ingredient);
            }
            this.cooker = cooker;
            this.finalProduct = finalProduct;
        }
    }
}
