using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Tao.OpenGl;

namespace Breakneck_Brigade.Graphics
{
    /// <summary>
    /// A particle that is represented by a 3D object
    /// </summary>
    class Particle3D : AParticle
    {
        /// <summary>
        /// The 3D object that represents this particle
        /// </summary>
        public AObject3D Obj3D;

        private VBO _singletonQuad;

        /// <summary>
        /// Instanciates a basic 3D particle with a blank 1x1x1 TexturedMesh as the as the object to render.
        /// Has the default texture.
        /// This particle has no velocity, no acceleration, and a lifetime of 10 seconds
        /// </summary>
        public Particle3D() 
            : base()
        {
            Obj3D = new TexturedMesh() { VBO = ((TexturedMesh)Renderer.Models["cube111"].Meshes[0]).VBO, Texture = Renderer.DefaultTexture };
        }

        /// <summary>
        /// Instanciates a basic 3D particle with a blank 1x1x1 TexturedMesh as the as the object to render.
        /// Has the specified texture and physics parameters
        /// </summary>
        /// <param name="texture"></param>
        public Particle3D(Texture texture)
            : base()
        {
            Obj3D = new TexturedMesh() { VBO = ((TexturedMesh) Renderer.Models["cube111"].Meshes[0]).VBO, Texture = texture };
        }

        /// <summary>
        /// Instanciates a basic 3D particle with the specified 3D object as the particle model. It will only use the first mesh in the model.
        /// </summary>
        public Particle3D(Model model)
            : base()
        {
            Obj3D = (TexturedMesh) model.Meshes[0];
        }

        public Particle3D(Model model, Texture texture)
            : base()
        {
            Obj3D = new TexturedMesh() { VBO = ((TexturedMesh)model.Meshes[0]).VBO, Texture = texture };
        }

        public override void Update(float timestep)
        {
            base.Update(timestep);
        }

        public override void Render()
        {
            Gl.glPushMatrix();
            Gl.glMultMatrixf(_transform.glArray);
                Obj3D.Render();
            Gl.glPopMatrix();
        }
    }
}
