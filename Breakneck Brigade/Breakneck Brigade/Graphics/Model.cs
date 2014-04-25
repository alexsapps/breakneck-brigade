using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Tao.OpenGl;

namespace Breakneck_Brigade.Graphics
{
    /*
     * A container class for data associated with a model
     * Ryan George
     */
    class Model
    {
        public List<AObject3D>  Meshes { get; private set; }
        public Vector4          InitialScale { get; set;}

        public Model()
        {
            Meshes = new List<AObject3D>();
            InitialScale = new Vector4();
        }

        public void Render()
        {
            foreach(AObject3D m in Meshes)
            {
                m.Render();
            }
        }
    }
}
