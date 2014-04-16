using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

namespace Breakneck_Brigade.Graphics
{
    /// <summary>
    /// An abstract class representing a
    /// </summary>
    abstract class AMesh : AObject3D
    {
        /// <summary>
        /// A list of polygons that comprise the primative.
        /// </summary>
        public List<APolygon> Polygons;
        /// <summary>
        /// The texture ID glu assigned to this mesh's texture at load time.
        /// </summary>
        public int GluTextureID;
        /// <summary>
        /// The mode OpenGL should be set to in order to draw this mesh.
        /// </summary>
        public int GlDrawMode;

        /// <summary>
        /// Instanciates a mesh with no transformations applied and an empty list of polygons.
        /// </summary>
        public AMesh() : base()
        {
            Polygons        = new List<APolygon>();
        }

        /// <summary>
        /// Just sets the transform of the object, leaving the Children and Polygons lists empty
        /// </summary>
        /// <param name="trans"></param>
        public AMesh(Matrix4 trans) : base(trans)
        {
            Polygons        = new List<APolygon>();
        }

        /// <summary>
        /// Sets all member variables of the mesh
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="children"></param>
        /// <param name="polys"></param>
        public AMesh(Matrix4 trans, List<AObject3D> children, List<APolygon> polys) : base(trans, children)
        {
            Polygons        = polys;
        }
    }
}
