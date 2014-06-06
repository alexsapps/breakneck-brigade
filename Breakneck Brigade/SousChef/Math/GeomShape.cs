using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public enum GeomShape
    {
        None,
        Box,
        Sphere,
        Cylinder,
        Capsule
    }

    public partial class BB
    {
        public static GeomShape ParseGeomShape(string str)
        {
            GeomShape shape = _parseGeomShape(str);
            if (shape == GeomShape.None)
                throw new Exception("ParseGeomShape not defined for " + str);
            return shape;
        }
        public static GeomShape ParseGeomShape(string str, GeomShape @default)
        {
            if (str == null)
                return @default;
            GeomShape shape = _parseGeomShape(str);
            if (shape == GeomShape.None)
                return @default;
            else
                return shape;
        }

        private static GeomShape _parseGeomShape(string str)
        {
            switch (str)
            {
                case "box":
                    return GeomShape.Box;
                case "sphere":
                    return GeomShape.Sphere;
                case "cylinder":
                    return GeomShape.Cylinder;
                default:
                    return GeomShape.None;
            }
        }
    }
}
