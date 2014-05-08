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
        /// A set of buffers that comprises the object
        /// </summary>
        public VBO VBO;

        /// <summary>
        /// Instanciates a mesh with no transformations applied and newly allocated VBO buffers.
        /// </summary>
        public AMesh() : base()
        {
            VBO        = new VBO();
        }

        /// <summary>
        /// Just sets the transform of the object, leaving the Children and buffers empty
        /// </summary>
        /// <param name="trans"></param>
        public AMesh(Matrix4 trans) : base(trans)
        {
            VBO        = new VBO();
        }

        /// <summary>
        /// Sets all member variables of the mesh
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="children"></param>
        /// <param name="polys"></param>
        public AMesh(Matrix4 trans, List<AObject3D> children, VBO buf) : base(trans, children)
        {
            VBO        = buf;
        }
    }
}
