using BulletSharp;
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

        /// <summary>
        /// Gets or set the lsit of euler angles to rotate this object.
        /// </summary>
        public Vector3 Euler { get; set; }

        /// <summary>
        /// Get or set the initial position of the object.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Creates a new initial geometry info class.
        /// </summary>
        public GeometryInfo()
        {
            this.Shape = GeomShape.Box;
            this.Euler = new Vector3(0, 0, 0);
            this.Position = new Vector3(0, 0, 0);
            this.Sides = new float[] { 500, 10, 500 };
        }
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
