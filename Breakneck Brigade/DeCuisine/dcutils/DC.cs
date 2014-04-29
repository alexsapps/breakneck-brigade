using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.Ode;
using System.IO;

namespace DeCuisine
{
    public static partial class DC
    {
        public static void Write(this BinaryWriter stream, Ode.dVector3 value)
        {
            // We need to flip the z and the y coordinate to make it fit with 
            // opengl coordinate system.
            stream.Write((float)value.X);
            stream.Write((float)value.Z);
            stream.Write((float)value.Y);
        }
        //ReadCoordinate is in client program
    }
}
