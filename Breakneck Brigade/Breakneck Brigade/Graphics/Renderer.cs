using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using SousChef;

namespace Breakneck_Brigade.Graphics
{
    class Renderer
    {
        private Matrix4         world;
        private List<Model>     objects; //Change to RenderableGameObjects later

        /// <summary>
        /// A singleton gluQuadric for use in Glu primative rendering functions
        /// </summary>
        public static Glu.GLUquadric gluQuadric = Glu.gluNewQuadric();
        /// <summary>
        /// A singleton gluTesselator for Glu tesselation
        /// </summary>
        public static Glu.GLUtesselator gluTesselator = Glu.gluNewTess();

        public Renderer()
        {
            world   = new Matrix4();
            objects = new List<Model>();
        }

        public void Render()
        {
            foreach(Model m in objects)
            {
                m.Render();
            }
        }
    }
}
