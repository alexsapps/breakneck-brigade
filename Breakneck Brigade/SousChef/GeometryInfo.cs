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
        public float[] Size { get; set; }
        public float[] ModelScale { get; protected set; }
        public float Friction { get; set; }
        public float RollingFriction { get; set; }
        public float Restitution { get; set; }
        public float AngularDamping { get; set; }
        public float Orientation { get; set; }

        private string _model;
        public string Model
        {
            get
            {
                return _model;
            }
            set
            {
                _model = value;
                var scaleVector = BB.modelParser.ScaleVector;
                var vertMinMax = value != null && scaleVector.ContainsKey(value) ? scaleVector[value] : scaleVector["bread"];
                ModelScale = new float[] { (vertMinMax[1].X - vertMinMax[0].X) / 2, (vertMinMax[1].Y - vertMinMax[0].Y) / 2, (vertMinMax[1].Z - vertMinMax[0].Z) / 2 };
            }
        }

        /// <summary>
        /// Gets or set the lsit of euler angles to rotate this object.
        /// </summary>
        public Vector3 Euler { get; set; }

        /// <summary>
        /// Creates a new initial geometry info class.
        /// </summary>
        public GeometryInfo()
        {
            this.Shape = GeomShape.Box;
            this.Euler = new Vector3(0, 0, 0);
            this.Size = new float[] { 500, 10, 500 };
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
