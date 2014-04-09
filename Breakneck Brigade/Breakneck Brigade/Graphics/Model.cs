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
        public  Matrix4             Transformation  { get; set; }
        public  List<TexturedMesh>  Meshes          { get; private set; }
        //public  TexturedMesh        Root            { get { return Meshes[0]; } private set;}

        public Model()
        {
            Meshes  = new List<TexturedMesh>();
            //Root = null;
        }

        public void LoadModelFromFile(string filename)
        {
            //Collada parsing method calls here
        }

        public void Render()
        {
            Gl.glPushMatrix();
            Gl.glLoadMatrixf(Transformation.glArray);
            foreach(TexturedMesh m in Meshes)
            {
                m.Render();
            }
            Gl.glPopMatrix();
        }
    }
}
