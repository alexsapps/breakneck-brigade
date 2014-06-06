using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breakneck_Brigade
{
    class ClientGoal : IComparable<ClientGoal>
    {
        public IngredientType Ingredient { get; set; }
        public bool Expiring { get; set; }
        public ClientGoal(IngredientType  ingredient, bool expiring)
        {
            this.Ingredient = ingredient;
            this.Expiring = expiring;
        }

        public int CompareTo(ClientGoal other)
        {
            return Ingredient.CompareTo(other.Ingredient);
        }
    }
}
