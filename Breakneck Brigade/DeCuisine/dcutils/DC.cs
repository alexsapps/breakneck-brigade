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
            // We need to flip the z and the y coordinate to make it fit with 
            // opengl coordinate system.
            stream.Write((float)value.X);
            stream.Write((float)value.Z);
            stream.Write((float)value.Y);
        }

        public static Coordinate ReadCoordinate(this BinaryReader stream)
        {
            return new Coordinate(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
        }
    }
}
