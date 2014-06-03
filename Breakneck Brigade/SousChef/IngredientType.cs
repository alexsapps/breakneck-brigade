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

        public IngredientType(string name, string friendlyName, GeometryInfo geomInfo, int defaultPoints) 
            : base(name, friendlyName, geomInfo)
        {
            this.DefaultPoints = defaultPoints;
        }
    }
}
