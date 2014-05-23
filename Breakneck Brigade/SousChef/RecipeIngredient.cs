using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class RecipeIngredient
    {
        public IngredientType Ingredient;
        public bool Optional;
        public RecipeIngredient(IngredientType ing, bool optional)
        {
            this.Ingredient = ing;
            this.Optional = optional;
        }
    }
}
