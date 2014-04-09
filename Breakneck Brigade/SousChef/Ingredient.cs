using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class Ingredient : GameObject
    {
        public string Name { get; set; }
    
        public Ingredient(int id, string name) 
            : base(id)
        {
            this.Name = name;
        }

        public override void Update()
        {

        }

    }
}
