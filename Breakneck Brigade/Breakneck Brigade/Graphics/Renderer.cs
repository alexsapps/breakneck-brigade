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
        /// <summary>
        /// A mapping of filename to Model
        /// </summary>
        public static Dictionary<string, Model>     Model       = new Dictionary<string, Model>();
        /// <summary>
        /// A mapping of filename to Texture
        /// </summary>
        public static Dictionary<string, Texture>   Textures    = new Dictionary<string, Texture>();



        private Matrix4         world;
        private List<Model>     objects;

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
