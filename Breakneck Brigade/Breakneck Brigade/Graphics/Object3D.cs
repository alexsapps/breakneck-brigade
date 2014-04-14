using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Tao.OpenGl;

namespace Breakneck_Brigade.Graphics
{
    /// <summary>
    /// A common interface for 3D representations of objects should inherit from. Implements a very simple scene graph-like interface: all
    /// children's transformations are relative to their parent's transformations.
    /// </summary>
    abstract class Object3D
    {
        /// <summary>
        /// A singleton gluQuadric for use in Glu primative rendering functions
        /// </summary>
        protected static Glu.GLUquadric gluQuadric = Glu.gluNewQuadric();
        /// <summary>
        /// The transformation for this mesh, relative to its parent. If this is the
        /// root node, set this to the identity matrix.
        /// </summary>
        public Matrix4          Transformation;
        /// <summary>
        /// Direct children of this node. All children inherit their parents' transformations
        /// </summary>
        public List<Object3D>   Children;

        /// <summary>
        /// GL calls to actually render this object. Rendering is done relative to the object's parents' transformations
        /// </summary>
        public abstract void Render();
    }
}
