using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    /// <summary>
    /// Holds all the information necessary for creating a game object's physical properites.
    /// </summary>
    public class GeometryInfo
    {
        public float Mass { get; set; }
        public GeomShape Shape { get; set; }
        public float[] Sides { get; set; }
        public float Friction { get; set; }
        public float Restitution { get; set; }
    }

    //public class RigidBodyGeometryInfo : GeometryInfo
    //{
    //    public float Mass { get; set; }
    //    public float Friction { get; set; }
    //    public float Restitution { get; set; }
    //}
    //public class BoxGeometryInfo : RigidBodyGeometryInfo
    //{
    //    public float Side1 { get; set; }
    //    public float Side2 { get; set; }
    //    public float Side3 { get; set; }
    //}
    //public class SphereGeometryInfo : RigidBodyGeometryInfo
    //{
    //    public float Side1 { get; set; }
    //    public float Side2 { get; set; }
    //}
    //public class PlaneGeometryInfo : GeometryInfo
    //{

    //}
    
}
