using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Tao.OpenGl;

namespace Breakneck_Brigade.Graphics
{
    class ColoredPolygon : APolygon
    {
        Vector4 Color;
        public ColoredPolygon() : base()
        {
            Color = new Vector4(1.0, 1.0, 1.0, 1.0);
        }
        public ColoredPolygon(float red, float green, float blue, float alpha) : base()
        {
            Color = new Vector4(red, green, blue, alpha);
        }

        public override void Render()
        {
            Gl.glPushMatrix();
                //Store previous color
                float[] prevColor = new float[4];
                Gl.glGetFloatv(Gl.GL_CURRENT_COLOR, prevColor);
                Gl.glColor4f(Color[0], Color[1], Color[2], Color[3]);

                foreach(Vertex v in Vertexes)
                {
                    Gl.glVertex3f(v.Position[0],v.Position[1],v.Position[2]);
                    Gl.glNormal3f(v.Normal[0],v.Normal[1],v.Normal[2]);
                }

                //Restore previous color
                Gl.glColor4fv(prevColor);
            Gl.glPopMatrix();
        }
    }
}
