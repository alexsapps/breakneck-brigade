using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class TerrainType : GameObjectType
    {
        public TerrainType(string name, string friendlyName, GeometryInfo geomInfo) : base(name, friendlyName, geomInfo)
        {

        }
    }
}
