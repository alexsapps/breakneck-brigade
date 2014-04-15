using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;

namespace Breakneck_Brigade.Graphics
{
    abstract class APolygon
    {
        public List<Vertex> Vertexes;
        public APolygon()
        {
            Vertexes = new List<Vertex>();
        }

        /// <summary>
        /// Renders this polygon to the screen.
        /// 
        /// This method should always be called after a call to 
        /// glBegin([the shape type for the mesh associated with this polygon])
        /// in its parent mesh's render method
        /// </summary>
        public abstract void Render();
    }
}
