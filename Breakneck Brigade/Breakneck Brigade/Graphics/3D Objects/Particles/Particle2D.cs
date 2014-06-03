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
    /// A particle that is represented by a 2D quad
    /// </summary>
    class Particle2D : AParticle
    {
        /// <summary>
        /// The 3D object that represents this particle
        /// </summary>
        public AObject3D Obj3D;

        private VBO _singletonQuad;

        /// <summary>
        /// Instanciates a basic 3D particle with a blank 1x1x0 TexturedMesh as the as the object to render.
        /// Has the default texture.
        /// This particle has no velocity, no acceleration, and a lifetime of 10 seconds
        /// </summary>
        public Particle2D() 
            : base()
        {
            if(_singletonQuad == null)
            {
                _singletonQuad = VBO.MakeFilledCenteredQuad();
                _singletonQuad.LoadData();
            }
            VBO quad = _singletonQuad;
            Obj3D = new TexturedMesh() { VBO = quad, Texture = Renderer.DefaultTexture};
        }

        /// <summary>
        /// Instanciates a basic 3D particle with a blank 1x1x0 TexturedMesh as the as the object to render.
        /// Has the specified texture and physics parameters
        /// </summary>
        /// <param name="texture"></param>
        public Particle2D(Texture texture)
            : base()
        {
            if (_singletonQuad == null)
            {
                _singletonQuad = VBO.MakeFilledCenteredQuad();
                _singletonQuad.LoadData();
            }
            VBO quad = _singletonQuad;
            Obj3D = new TexturedMesh() { VBO = quad, Texture = texture };
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
