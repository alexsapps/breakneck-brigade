using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ConfigSalad
    {
        public Dictionary<string, IngredientType> Ingredients { get; set; }
        public Dictionary<string, Recipe> Recipes { get; set; }
        public Dictionary<string, CookerType> Cookers { get; set; }
        public Dictionary<string, TerrainType> Terrains { get; set; }
        public Dictionary<string, Vector4[]> ModelScale { get; set; }

        public string Hash { get; set; }
    }
}
