using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class Cooker : GameObject
    {
        public CookerType Type { get; set; }
        public List<Recipe> Contents { get; private set; }

        public Cooker (int id, CookerType type)
            : base(id)
        {
            this.Type = type;
            Contents = new List<Recipe>();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
