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
        public Matrix4          ModelMatrix { get; set;}
        public List<AObject3D>  Meshes { get; private set; }

        public Model()
        {
            Meshes = new List<AObject3D>();
            ModelMatrix = new Matrix4();
        }

        public Model(Matrix4 mm)
        {
            Meshes = new List<AObject3D>();
            ModelMatrix = mm;
        }

        public void Render()
        {
            Gl.glPushMatrix();
            Gl.glMultMatrixf(ModelMatrix.glArray);
            foreach(AObject3D m in Meshes)
            {
                m.Render();
            }
            Gl.glPopMatrix();
        }
    }
}
