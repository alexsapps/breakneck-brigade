using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class IngredientType : GameObjectType, IComparable<IngredientType>
    {
        public int DefaultPoints { get; set; }
        public double powerUp { get; set; }
        public IngredientType(string name, string friendlyName, GeometryInfo geomInfo, int defaultPoints, double powerUp) 
            : base(name, friendlyName, geomInfo)
        {
            this.DefaultPoints = defaultPoints;
            this.powerUp = powerUp;
        }

        public int CompareTo(IngredientType other)
        {
            return DefaultPoints - other.DefaultPoints;
        }
    }
}
