using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class CookerType : GameObjectType
    {
        public Dictionary<string, Recipe> Recipes { get; set; }
        public HashSet<string> ValidIngredients { get; set; }
        public Dictionary<string, HashSet<Recipe>> RecipeHash; // Fast dict to keep track of what ingredients are a part of what recipe


        public CookerType(string name, GeometryInfo geomInfo, List<Recipe> recipes)
            : base(name, geomInfo)
        {
            Debug.Assert(recipes != null);
            
            Recipes = new Dictionary<string, Recipe>();
            ValidIngredients = new HashSet<string>();
            RecipeHash = new Dictionary<string, HashSet<Recipe>>();

            foreach (var recipe in recipes)
            {
                foreach (var ing in recipe.Ingredients)
                {
                    ValidIngredients.Add(ing.Ingredient.Name);
                    addToRecipeHash(ing.Ingredient.Name, recipe);
                }
                Recipes.Add(recipe.hash(), recipe);
            }
        }

        private void addToRecipeHash(string name, Recipe recipe)
        {
            HashSet<Recipe> tmp;
            RecipeHash.TryGetValue(name, out tmp);
            if (tmp != null)
                RecipeHash[name].Add(recipe);
            else
            {
                tmp = new HashSet<Recipe>();
                tmp.Add(recipe);
                RecipeHash.Add(name, tmp);
            }
        }

    }
}
