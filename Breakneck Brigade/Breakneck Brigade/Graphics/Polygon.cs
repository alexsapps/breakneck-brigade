using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;

namespace Breakneck_Brigade.Graphics
{
    class Polygon
    {
        public List<Vertex> Vertexes;
        public Polygon()
        {
            Vertexes = new List<Vertex>();
        }
        public void Render()
        {
            foreach(Vertex v in Vertexes)
            {
                Gl.glVertex3f(v.Position[0], v.Position[1], v.Position[2]);
                Gl.glNormal3f(v.Normal[0], v.Normal[1], v.Normal[2]);
                //Gl.glTexCoord2f(v.TextureCoordinates[0], v.TextureCoordinates[1]);
            }
        }
    }
}
