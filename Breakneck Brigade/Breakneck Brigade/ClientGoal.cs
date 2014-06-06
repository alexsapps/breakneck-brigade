using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breakneck_Brigade
{
    class ClientGoal
    {
        public IngredientType Ingredient { get; set; }
        public bool Expiring { get; set; }
        public ClientGoal(IngredientType  ingredient, bool expiring)
        {
            this.Ingredient = ingredient;
            this.Expiring = expiring;
        }
    }
}
