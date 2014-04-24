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
        public string Model { get; set; }

        public CookerType(string name, GeometryInfo geomInfo, List<Recipe> recipes, string model)
            : base(name, geomInfo)
        {
            Debug.Assert(recipes != null);
            
            Recipes = new Dictionary<string, Recipe>();
            ValidIngredients = new HashSet<string>();

            foreach (var recipe in recipes)
            {
                foreach (var ing in recipe.Ingredients)
                {
                    ValidIngredients.Add(ing.Name);
                }
                Recipes.Add(recipe.hash(), recipe);
            }
            this.Model = model;
        }

    }
}
