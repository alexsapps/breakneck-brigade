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

        public Renderer()
        {
            world   = new Matrix4();
            objects = new List<Model>();
#if PROJECT_GRAPHICS_MODE
            Model cube = new Model();

            //Verts
            Vertex v1 = new Vertex(-1.0,-1.0,-1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            Vertex v2 = new Vertex(-1.0, 1.0,-1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            Vertex v3 = new Vertex( 1.0,-1.0,-1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            Vertex v4 = new Vertex( 1.0, 1.0,-1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            Vertex v5 = new Vertex(-1.0,-1.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            Vertex v6 = new Vertex( 1.0,-1.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            Vertex v7 = new Vertex(-1.0, 1.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            Vertex v8 = new Vertex( 1.0, 1.0, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0);

            //Front face
            TexturedMesh front = new TexturedMesh();
            front.GlDrawMode = Gl.GL_QUADS;
            Polygon frontPoly = new Polygon();
            frontPoly.Vertexes.Add(v5);
            frontPoly.Vertexes.Add(v7);
            frontPoly.Vertexes.Add(v8);
            frontPoly.Vertexes.Add(v6);

            front.Polygons.Add(frontPoly);
            cube.Meshes.Add(front);

            //Back face
            TexturedMesh back = new TexturedMesh();
            front.GlDrawMode = Gl.GL_QUADS;
            Polygon backPoly = new Polygon();
            frontPoly.Vertexes.Add(v5);
            frontPoly.Vertexes.Add(v7);
            frontPoly.Vertexes.Add(v8);
            frontPoly.Vertexes.Add(v6);

            front.Polygons.Add(backPoly);
            cube.Meshes.Add(back);
            
#endif
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
