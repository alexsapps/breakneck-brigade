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
    class TexturedMesh
    {
        /// <summary>
        /// A list of polygons that comprise the primative.
        /// </summary>
        public List<Polygon>        Polygons;
        /// <summary>
        /// The texture ID glu assigned to this mesh's texture at load time.
        /// </summary>
        public int                  GluTextureID;
        /// <summary>
        /// The mode OpenGL should be set to in order to draw this mesh.
        /// </summary>
        public int                  GlDrawMode;
        /// <summary>
        /// The transformation for this mesh, relative to its parent. If this is the
        /// root node, set this to the identity matrix.
        /// </summary>
        public Matrix4              Transformation;
        /// <summary>
        /// Direct children of this node.
        /// </summary>
        public List<TexturedMesh>   Children; //TODO: Might need to have textured mesh inherit from a common interface if we want to use GLU prims later
        public TexturedMesh()
        {
            Polygons = new List<Polygon>();
        }

        public void Render()
        {
            Gl.glLoadMatrixf(Transformation.glArray);
            Gl.glPushMatrix();
            //Gl.glBindTexture(Gl.GL_TEXTURE_2D, GluTextureID);
            foreach(Polygon p in Polygons)
            {
                p.Render();
            }
            foreach(TexturedMesh c in Children)
            {
                c.Render();
            }
            Gl.glPopMatrix();
        }
    }
}
