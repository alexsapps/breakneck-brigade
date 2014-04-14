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
        public List<Recipe> Recipes { get; set; } //Not an Infiniter cooker
        public string Model { get; set; }

        public CookerType(string name, List<Recipe> recipes, string model)
            : base(name)
        {
            Debug.Assert(recipes != null);

            this.Recipes = recipes;
            this.Model = model;
        }

    }
}
