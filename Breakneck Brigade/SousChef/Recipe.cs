using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class Recipe 
    {
        public List<IngredientType> Ingredients { get; private set; }
        public IngredientType FinalProduct;
        
        public Recipe(List<IngredientType> ingredients, IngredientType finalProduct)
        {
            this.Ingredients = ingredients;
            this.FinalProduct = finalProduct;
        }
    }
}
