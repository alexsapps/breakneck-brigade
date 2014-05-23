using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public enum TerrainName
    {
        WALL,
        FLOOR,
        CEILING,
    }
    public class TerrainType : GameObjectType
    {
        public static string TerrainNameToString(TerrainName val)
        {
            switch(val)
            {
                case TerrainName.WALL:
                    return "wall";
                case TerrainName.FLOOR:
                    return "floor";
                case TerrainName.CEILING:
                    return "ceiling";
                default:
                    throw new Exception("TerrainName value has no associated string!");
            }
        }

        public static TerrainName StringToTerrainName(string val)
        {
            switch (val)
            {
                case "wall":
                    return TerrainName.WALL;
                case "floor":
                    return TerrainName.FLOOR;
                case "ceiling":
                    return TerrainName.CEILING;
                default:
                    throw new Exception("string value has no associated TerrainName!");
            }
        }


        public TerrainType(TerrainName name, GeometryInfo geomInfo) 
            : base(TerrainNameToString(name), geomInfo)
        {

        }
    }
}
