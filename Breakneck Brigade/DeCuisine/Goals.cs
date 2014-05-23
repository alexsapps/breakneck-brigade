using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

namespace DeCuisine
{
    public class Goal
    {
        public int Points { get; set; }
        public IngredientType GoalIng { get; set; }
        //delegate CompleteFunction;

        public Goal(int points, IngredientType ing)
        {
            this.Points = points;
            this.GoalIng = ing;
        }


    }
}
