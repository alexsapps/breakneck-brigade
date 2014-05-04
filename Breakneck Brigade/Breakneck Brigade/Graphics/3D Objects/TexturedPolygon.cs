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

        public TexturedPolygon(int glDrawMode) : base()
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
                foreach(Vertex v in Vertexes)
                {
                    Gl.glColor3f(1.0f, 1.0f,1.0f);

                    if (v.TextureCoordinates != null)
                        Gl.glTexCoord2f(v.TextureCoordinates[0], v.TextureCoordinates[1]);
                    if (v.Normal != null)
                        Gl.glNormal3f(v.Normal[0], v.Normal[1], v.Normal[2]);
                    Gl.glVertex3f(v.Position[0],v.Position[1],v.Position[2]);
                } 
            Gl.glEnd();
        }
    }
}
