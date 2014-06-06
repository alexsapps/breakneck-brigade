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
        public Recipe EndGoal { get; set; }
        public string GoalString { get; set; } // the string that is used to convey what the goal is
        public bool Expiring { get; set; }
        //delegate CompleteFunction;

        public Goal(int points, Recipe goal, int complexity)
        {
            this.Points = points;
            this.EndGoal = goal;
            if (complexity > 0)
            {
                // add optional goals as well.
            }
        }


    }
}
