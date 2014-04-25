using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public enum GeomShape
    {
        Box,
        Sphere
    }

    public partial class BB
    {
        public static GeomShape ParseGeomShape(string str)
        {
            switch (str)
            {
                case "box":
                    return GeomShape.Box;
                case "sphere":
                    return GeomShape.Sphere;
                default:
                    throw new Exception("ParseGeomShape not defined for " + str);
            }
        }
    }
}
