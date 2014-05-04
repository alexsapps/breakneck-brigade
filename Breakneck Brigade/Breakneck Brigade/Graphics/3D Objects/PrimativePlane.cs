using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using SousChef;

namespace Breakneck_Brigade.Graphics
{
    /// <summary>
    /// Renders a simple, untesselated horizontal plane. Note this will not
    /// accurately represent lighting, as the surface is a single quad.
    /// </summary>
    class PrimativePlane : AObject3D
    {
        public float Height;

        private const float PLANE_WIDTH = 1000; 
        
        public PrimativePlane(float height) 
            : base()
        {
            Height = height;
        }

        public PrimativePlane(float height, Matrix4 trans)
            : base(trans)
        {
            Height = height;
        }

        public PrimativePlane(float height, List<AObject3D> children)
            : base(children)
        {
            Height = height;
        }

        public PrimativePlane(float height, Matrix4 trans, List<AObject3D> children)
            : base(trans, children)
        {
            Height = height;
        }

        public override void Render()
        {
            Gl.glBegin(Gl.GL_QUADS);
                Gl.glColor3f(1.0f, 1.0f, 1.0f);

                Gl.glNormal3f(0.0f, 1.0f, 0.0f);

                Gl.glVertex3f(-PLANE_WIDTH, Height, -PLANE_WIDTH);
                Gl.glVertex3f(-PLANE_WIDTH, Height, PLANE_WIDTH);
                Gl.glVertex3f(PLANE_WIDTH, Height, PLANE_WIDTH);
                Gl.glVertex3f(PLANE_WIDTH, Height, -PLANE_WIDTH);
            Gl.glEnd();
        }
    }
}
