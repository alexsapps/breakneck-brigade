using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class IngredientType : GameObjectType
    {
        public int DefaultPoints { get; set; }

        public IngredientType(string name, GeometryInfo geomInfo, int defaultPoints) 
            : base(name, geomInfo)
        {
            this.DefaultPoints = defaultPoints;
        }
    }
}
