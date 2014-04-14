using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class Ingredient : GameObject
    {
        public IngredientType Type { get; set; }
        public int Cleanliness { get; set; }

        public Ingredient (int id, IngredientType type)
            : base(id)
        {
            this.Type = type;
            Cleanliness = 0;
        }

        public override void Update()
        {
            this.Position[1]--; //TODO: not this
        }
    }
}
