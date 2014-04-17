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
        public TexturedPolygon() : base()
        {
            
        }

        public override void Render()
        {
            Gl.glPushMatrix();
                foreach(Vertex v in Vertexes)
                {
                    Gl.glColor3f(1.0f, 1.0f,1.0f);

                    Gl.glTexCoord2f(v.TextureCoordinates[0], v.TextureCoordinates[1]);
                    if (v.Normal != null)
                        Gl.glNormal3f(v.Normal[0], v.Normal[1], v.Normal[2]);
                    if(v.TextureCoordinates != null)
                        Gl.glVertex3f(v.Position[0],v.Position[1],v.Position[2]);
                }
            Gl.glPopMatrix();
        }
    }
}
