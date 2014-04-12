using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class Cooker : GameObject
    {

        public string Name { get; set; }
        public List<Ingredient> Ingredients;
        public List<Recipe> Recipes; //Not an Infiniter cooker

        public Cooker(int id, string name)
            : base(id)
        {
            this.Name = name;
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }


    }
}
