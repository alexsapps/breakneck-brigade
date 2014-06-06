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
        public int nCount; //number of instances of ingredient needed for this recipe
        public int nOptional; //number of optional/extra instances that can count for extra points in this recipe
        public int total { get { return this.nCount + this.nOptional;} } // convenience!
        public int score;
        public RecipeIngredient(IngredientType ing, int count, int optional)
        {
            this.Ingredient = ing;
            this.nCount = count;
            this.nOptional = optional;
        }

        public int CalculateScore(int matchingCount)
        {
            // we could make this more calculated
            return this.Ingredient.DefaultPoints * matchingCount; 
        }
    }
}
