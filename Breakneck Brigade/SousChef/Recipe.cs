using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class Recipe 
    {
        public string Name { get; private set; } //in case FinalProduct does not uniquely determine Recipe
        public string FriendlyName { get; private set; }
        public List<CookerType> UsableCookers { get; private set; }
        public List<RecipeIngredient> Ingredients { get; private set; } //if this ever changes, make sure to set _hash=null
        public IngredientType FinalProduct;
        
        public Recipe(string name, string friendlyName, List<RecipeIngredient> ingredients, IngredientType finalProduct)
        {
            this.Name = name;
            this.FriendlyName = name;
            this.Ingredients = ingredients;
            this.FinalProduct = finalProduct;
            this.UsableCookers = new List<CookerType>();

            int sum = 0;
            foreach (var ingredient in ingredients)
                sum += ingredient.Ingredient.DefaultPoints;

            hash(); //compute now
        }

        string _hash;
        public string hash()
        {
            if(_hash == null)
            {
                _hash = Hash(Ingredients);
            }
            return _hash;
        }

        public static string Hash(List<RecipeIngredient> list)
        {
            StringBuilder names = new StringBuilder();
            foreach (var recipeIngredient in list)
            {
                names.Append(recipeIngredient.Ingredient.Name);
                names.Append("|");
            }
            return names.ToString();
        }
    }
}
