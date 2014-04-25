using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using Tao.Glfw;
using SousChef;

namespace Breakneck_Brigade.Graphics
{
    public enum ParticleType
    {
        WATER
    }

    abstract class AParticleSpawner
    {
        /// <summary>
        /// Current position of the particle
        /// </summary>
        public  Vector4 Position { get; set; }
        
        private Matrix4         _transform;

        protected           List<AParticle> _particles;
        protected           double          _lastUpdateTime;
        protected           double          _currentUpdateTime;
        protected           bool            _spawning;
        protected           int             _currentNumberOfParticles;
        protected   const   int             MAX_PARTICLES = 99;

        public AParticleSpawner()
        {
            _particles                  = new List<AParticle>();
            _lastUpdateTime             = -1.0;
            _transform                  = new Matrix4();
            _spawning                   = false;
            _currentNumberOfParticles   = 0; 
            Position                    = new Vector4();
        }

        public AParticleSpawner(Vector4 position)
        {
            _particles                  = new List<AParticle>();
            _lastUpdateTime             = -1.0;
            _transform                  = new Matrix4();
            _spawning                   = false;
            _currentNumberOfParticles   = 0;
            Position                    = position;
        }

        public virtual void StartSpawning()
        {
            _spawning = true;
        }
        public virtual void StopSpawning()
        {
            _spawning = false;
        }

        /// <summary>
        /// Updates all the particles this spawner has created.
        /// 
        /// This base function moves all the particles according to their accelerations,
        /// and should be called if you want your particles to have normal kinematic motion.
        /// </summary>
        public virtual void Update()
        {
            if(_lastUpdateTime == -1.0)
            {
                _currentUpdateTime = Glfw.glfwGetTime();
            }
            _lastUpdateTime = _currentUpdateTime;
            _currentUpdateTime = Glfw.glfwGetTime();

            float timestep = (float) (_currentUpdateTime - _lastUpdateTime);

            foreach(AParticle particle in _particles)
            {
                particle.Update(timestep);
            }

            _transform.TranslationMat(Position.X, Position.Y, Position.Z);
        }

        public virtual void Render()
        {
            Gl.glPushMatrix();
            Gl.glMultMatrixf(_transform.glArray);

                foreach(AParticle particle in _particles)
                {
                    particle.Render();
                }

            Gl.glPopMatrix();
        }

        public virtual void DestroyAll()
        {
            _particles.Clear();
        }

        protected AParticle CreateParticle(ParticleType pt)
        {
            switch(pt)
            {
                case ParticleType.WATER:
                    return new Particle3D(Position, new Vector4(), new Vector4(), .25f, 10000.0f);
                default:
                    return null;
            }
        }
    }
}
