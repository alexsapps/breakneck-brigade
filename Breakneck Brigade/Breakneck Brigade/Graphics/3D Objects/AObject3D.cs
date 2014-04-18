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
    abstract class AObject3D
    {
        /// <summary>
        /// The transformation for this mesh, relative to its parent. If this is the
        /// root node, set this to the identity matrix.
        /// </summary>
        public Matrix4          Transformation;
        /// <summary>
        /// Direct children of this node. All children inherit their parents' transformations
        /// </summary>
        public List<AObject3D>   Children;

        public AObject3D()
        { 
            Transformation  = new Matrix4();
            Children        = new List<AObject3D>();
        }
        public AObject3D(Matrix4 trans)
        {
            Transformation  = trans;
            Children        = new List<AObject3D>();
        }
        public AObject3D(List<AObject3D> children)
        {
            Transformation  = new Matrix4();
            Children        = children;
        }
        public AObject3D(Matrix4 trans, List<AObject3D> children)
        {
            Transformation  = trans;
            Children        = children;
        }
         

        /// <summary>
        /// GL calls to actually render this object. Rendering is done relative to the object's parents' transformations
        /// </summary>
        public abstract void Render();
    }
}
