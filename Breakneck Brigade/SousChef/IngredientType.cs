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
        public string Model { get; set; } //TODO: 

        public IngredientType(string name, GeometryInfo geomInfo, int defaultPoints, string model) 
            : base(name, geomInfo)
        {
            this.Model = model;
            this.DefaultPoints = defaultPoints;
        }
    }
}
