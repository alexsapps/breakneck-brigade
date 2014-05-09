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
    /// A container for an OpenGL primative represented as a collection of polygons and a 
    /// </summary>
    class TexturedMesh : AMesh
    {
        /// <summary>
        /// The texture for this mesh
        /// </summary>
        public Texture Texture;

        /// <summary>
        /// Instanciates a mesh with no transformations applied and an empty list of polygons.
        /// Assigns a blank texture to the object
        /// </summary>
        public TexturedMesh() : base()
        {
            Texture = null;
        }
        
        /// <summary>
        /// Instanciates a mesh with no transformations applied and an empty list of polygons.
        /// Assigns a blank texture to the object
        /// </summary>
        /// <param name="texture"></param>
        public TexturedMesh(Texture texture) : base()
        {
            Texture = texture;
        }        

        /// <summary>
        /// Just sets the transform of the object, leaving the Children and Polygons lists empty. 
        /// Assigns a blank texture to the object
        /// </summary>
        /// <param name="trans"></param>
        public TexturedMesh(Matrix4 trans) : base(trans)
        {
            Texture = null;
        }

        /// <summary>
        /// Just sets the transform of the object, leaving the Children and Polygons lists empty. 
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="texture"></param>
        public TexturedMesh(Matrix4 trans, Texture texture)
            : base(trans)
        {
            Texture = texture;
        }

        /// <summary>
        /// Sets all member variables of the mesh
        /// Assigns a blank texture to the object
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="children"></param>
        /// <param name="polys"></param>
        public TexturedMesh(Matrix4 trans, List<AObject3D> children, VBO buf) : base(trans, children, buf)
        {
            Texture = null;
        }

        /// <summary>
        /// Sets all member variables of the mesh
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="children"></param>
        /// <param name="polys"></param>
        /// <param name="texture"></param>
        public TexturedMesh(Matrix4 trans, List<AObject3D> children, VBO buf, Texture texture) : base(trans, children, buf)
        {
            Texture = texture;
        }

        /// <summary>
        /// Renders the mesh, setting the appropriate draw mode for the polygons.
        /// </summary>
        public override void Render()
        {
            Gl.glPushMatrix();
            Gl.glMultMatrixf(_transform.glArray);
            
            Texture.Bind();

            //Render parent
            VBO.Render();

            //Render children, preserving the parent transformations
            foreach(AObject3D c in Children)
            {
                c.Render();
            }
            Gl.glPopMatrix();
        }
    }
}
