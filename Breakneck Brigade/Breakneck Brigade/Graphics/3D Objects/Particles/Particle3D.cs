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
        AObject3D Obj3D;

        /// <summary>
        /// Instanciates a basic 3D particle with a 1.0 radius TexturedGluSphere as the object to render.
        /// This particle has no velocity, no acceleration, and a lifetime of 10 seconds
        /// </summary>
        public Particle3D() 
            : base()
        {
            Obj3D = new TexturedGluSphere(1.0, 5, 5);
        }

        /// <summary>
        /// Instanciates a basic 3D particle with the specified 3D object as the particle model
        /// </summary>
        public Particle3D(AObject3D obj3D)
            : base()
        {
            Obj3D = obj3D;
        }

        /// <summary>
        /// Instanciates a basic 3D particle with a 1.0 radius TexturedGluSphere as the object to render
        /// This particle has no velocity, no acceleration, and a lifetime of 10 seconds
        /// </summary>
        /// <param name="position"></param>
        /// <param name="velocity"></param>
        /// <param name="acceleration"></param>
        /// <param name="scale"></param>
        /// <param name="lifetime"></param>
        public Particle3D(Vector4 position, Vector4 velocity, Vector4 acceleration, float scale, float lifetime) :
            base(position, velocity, acceleration, scale, lifetime)
        {
            Obj3D = new TexturedGluSphere(1.0, 5, 5);
        }


        /// <summary>
        /// Instanciates a basic 3D particle with the specified 3D object as the particle model
        /// </summary>
        /// <param name="position"></param>
        /// <param name="velocity"></param>
        /// <param name="acceleration"></param>
        /// <param name="scale"></param>
        /// <param name="lifetime"></param>
        /// <param name="obj3D"></param>
        public Particle3D(Vector4 position, Vector4 velocity, Vector4 acceleration, float scale, float lifetime, AObject3D obj3D) :
            base(position, velocity, acceleration, scale, lifetime)
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
