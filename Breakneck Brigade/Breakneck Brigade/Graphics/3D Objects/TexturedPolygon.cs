using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Tao.OpenGl;

namespace Breakneck_Brigade.Graphics
{
    class TexturedPolygon : APolygon
    {
        /// <summary>
        /// The mode OpenGL should be set to in order to draw this mesh.
        /// </summary>
        public int GlDrawMode;

        public TexturedPolygon(int glDrawMode, int vertexCount) : base(vertexCount)
        {
            GlDrawMode = glDrawMode;
        }

        public override void Render()
        {
            //If if is THE first polygon we've rendered
            if(Renderer.CurrentDrawMode == -1)
            {
                Gl.glBegin(GlDrawMode);
                Renderer.CurrentDrawMode = GlDrawMode;
            }
            //If the last polygon we rendered is the same draw mode as this polygon
            if(Renderer.CurrentDrawMode != GlDrawMode)
            {
                Gl.glEnd();
                Gl.glBegin(GlDrawMode);
                Renderer.CurrentDrawMode = GlDrawMode;
            }
            Gl.glBegin(Renderer.CurrentDrawMode);
            Gl.glColor3f(1.0f, 1.0f, 1.0f); //this was in the loop before, maybe it should still be there
            for (int i = 0; i < Vertexes.Length; i++)
            {
                Vertex v = Vertexes[i];
                

                //if (v.TextureCoordinates != null)
                    Gl.glTexCoord2f(v.TextureCoordinates.X, v.TextureCoordinates.Y);
                //if (v.Normal != null)
                    Gl.glNormal3f(v.Normal.X, v.Normal.Y, v.Normal.Z);
                Gl.glVertex3f(v.Position.X, v.Position.Y, v.Position.Z);
            }
            Gl.glEnd();
        }
    }
}
