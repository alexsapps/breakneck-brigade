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

        /// <summary>
        /// Instanciates a basic 3D particle with a blank 1x1x0 TexturedMesh as the as the object to render.
        /// Has the default texture.
        /// This particle has no velocity, no acceleration, and a lifetime of 10 seconds
        /// </summary>
        public Particle3D() 
            : base()
        {
            VBO quad = VBO.MakeFilledCenteredQuad();
            quad.LoadData();
            Obj3D = new TexturedMesh() { VBO = quad, Texture = Renderer.DefaultTexture};
        }

        /// <summary>
        /// Spawns a particle with a blank 1x1x0 TexturedMesh as the as the object to render.
        /// Particle has the default texture.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="velocity"></param>
        /// <param name="acceleration"></param>
        /// <param name="scale"></param>
        /// <param name="lifetime"></param>
        public Particle3D(Vector4 position, Vector4 velocity, Vector4 acceleration, float scale, float lifetime)
            : base(position, velocity, acceleration, scale, lifetime)
        {
            VBO quad = VBO.MakeFilledCenteredQuad();
            quad.LoadData();
            Obj3D = new TexturedMesh() { VBO = quad, Texture = Renderer.DefaultTexture };
        }

        /// <summary>
        /// Instanciates a basic 3D particle with a blank 1x1x0 TexturedMesh as the as the object to render.
        /// Has the specified texture and physics parameters
        /// </summary>
        /// <param name="texture"></param>
        public Particle3D(Texture texture)
            : base()
        {
            VBO quad = VBO.MakeFilledCenteredQuad();
            quad.LoadData();
            Obj3D = new TexturedMesh() { VBO = quad, Texture = texture };
        }

        /// <summary>
        /// Instanciates a basic 3D particle with a blank 1x1x0 TexturedMesh as the as the object to render.
        /// Has the specified texture and physics parameters
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="position"></param>
        /// <param name="velocity"></param>
        /// <param name="acceleration"></param>
        /// <param name="scale"></param>
        /// <param name="lifetime"></param>
        public Particle3D(Texture texture, Vector4 position, Vector4 velocity, Vector4 acceleration, float scale, float lifetime)
            : base(position, velocity, acceleration, scale, lifetime)
        {
            VBO quad = VBO.MakeFilledCenteredQuad();
            quad.LoadData();
            Obj3D = new TexturedMesh() { VBO = quad, Texture = texture};
            //Obj3D = Renderer.Models["flour"].Meshes[0];
        }

        /// <summary>
        /// Instanciates a basic 3D particle with the specified 3D object as the particle model
        /// </summary>
        public Particle3D(AObject3D obj3D)
            : base()
        {
            Obj3D = obj3D;
        }

        public override void Update(float timestep)
        {
            base.Update(timestep);
        }

        public override void Render()
        {
            Gl.glLoadMatrixf(_transform.glArray);
            Gl.glPushMatrix();
                Obj3D.Render();
            Gl.glPopMatrix();
        }
    }
}
