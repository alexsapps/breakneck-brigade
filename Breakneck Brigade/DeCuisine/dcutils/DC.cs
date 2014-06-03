using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SousChef;

using BulletSharp;

namespace DeCuisine
{
    public static partial class DC
    {
        //ReadCoordinate is in client program
        public static Random random = new Random();

        public static void Write(this BinaryWriter stream, Vector3 value)
        {
            // opengl coordinate system.
            stream.Write((float)value.X);
            stream.Write((float)value.Y);
            stream.Write((float)value.Z);
        }

        public static SousChef.Vector4 ToVector4(this Vector3 vector)
        {
            return new SousChef.Vector4(vector.X, vector.Y, vector.Z);
        }
    }
}
