using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    /// <summary>
    /// simple, lightweight generic Coordinate class mainly for use by server
    /// </summary>
    public class Coordinate
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public Coordinate(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }


}
