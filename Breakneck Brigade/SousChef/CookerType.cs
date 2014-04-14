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
        public Dictionary<string, bool> ValidIngredients { get; set; } //bools not used, just need the lookup time of a dict
        public string Model { get; set; }

        public CookerType(string name, List<Recipe> recipes, string model)
            : base(name)
        {
            Debug.Assert(recipes != null);

            foreach (var recipe in recipes)
            {
                foreach (var ing in recipe.Ingredients)
                {
                    if (!ValidIngredients.ContainsKey(ing.Name))
                    {
                        //add to valid ingredient dict
                        ValidIngredients.Add(ing.Name, true);
                    }
                }
                //add recipe to hash by a string from the ingredients
                Recipes.Add(recipe.hash(), recipe);
            }
            this.Model = model;
        }

    }
}
