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
    class TexturedMesh : AObject3D
    {
        /// <summary>
        /// A list of polygons that comprise the primative.
        /// </summary>
        public List<TexturedPolygon>    Polygons;
        /// <summary>
        /// The texture ID glu assigned to this mesh's texture at load time.
        /// </summary>
        public int                      GluTextureID;
        /// <summary>
        /// The mode OpenGL should be set to in order to draw this mesh.
        /// </summary>
        public int                      GlDrawMode;

        /// <summary>
        /// Instanciates a mesh with no transformations applied and an empty list of polygons.
        /// </summary>
        public TexturedMesh()
        {
            Transformation  = new Matrix4();
            Children        = new List<AObject3D>();
            Polygons        = new List<TexturedPolygon>();
        }

        /// <summary>
        /// Just sets the transform of the object, leaving the Children and Polygons lists empty
        /// </summary>
        /// <param name="trans"></param>
        public TexturedMesh(Matrix4 trans)
        {
            Transformation  = trans;
            Children        = new List<AObject3D>();
            Polygons        = new List<TexturedPolygon>();
        }

        /// <summary>
        /// Sets all member variables of the mesh
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="children"></param>
        /// <param name="polys"></param>
        public TexturedMesh(Matrix4 trans, List<AObject3D> children, List<TexturedPolygon> polys)
        {
            Transformation  = trans;
            Children        = children;
            Polygons        = polys;
        }

        /// <summary>
        /// Renders the mesh, setting the appropriate draw mode for the polygons.
        /// </summary>
        public override void Render()
        {
            Gl.glPushMatrix();
            Gl.glMultMatrixf(Transformation.glArray);
            //Gl.glBindTexture(Gl.GL_TEXTURE_2D, GluTextureID);

            //Render parent
            foreach (TexturedPolygon p in Polygons)
            {
                Gl.glBegin(GlDrawMode);
                    p.Render();
                Gl.glEnd();
            }

            //Render children, preserving the parent transformations
            foreach(AObject3D c in Children)
            {
                c.Render();
            }
            Gl.glPopMatrix();
        }
    }
}
