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
    abstract class AParticleSpawner
    {
        /// <summary>
        /// Current position of the particle
        /// </summary>
        public  Vector4 Position { get; set; }
        
        private Matrix4         _transform;

        protected       List<AParticle> _particles;
        protected       List<AParticle> _toKill;
        protected       double          _lastSpawnTime;
        protected       double          _lastUpdateTime;
        protected       double          _currentUpdateTime;
        protected       bool            _spawning;
        /// <summary>
        /// The maximum amount of particles this spawner should have out at any time
        /// </summary>
        public          int             MaxParticles = 99;
        /// <summary>
        /// How often the particles should spawn in seconds
        /// </summary>
        public          double          spawnPeriod = .1000;

        public AParticleSpawner()
        {
            _particles                  = new List<AParticle>();
            _toKill                     = new List<AParticle>();
            _lastUpdateTime             = -1.0;
            _transform                  = new Matrix4();
            _spawning                   = false;
            Position                    = new Vector4();
        }

        public AParticleSpawner(Vector4 position)
        {
            _particles                  = new List<AParticle>();
            _toKill                     = new List<AParticle>();
            _lastUpdateTime             = -1.0;
            _transform                  = new Matrix4();
            _spawning                   = false;
            Position                    = position;
        }

        /// <summary>
        /// Turns on spawning. Can be used to run additional logic
        /// </summary>
        public virtual void StartSpawning()
        {
            _spawning = true;
            _lastSpawnTime = Glfw.glfwGetTime();
        }
        /// <summary>
        /// Turns off spawning. Can be used to run additional logic
        /// </summary>
        public virtual void StopSpawning()
        {
            _spawning = false;
        }

        /// <summary>
        /// Updates all the particles this spawner has created.
        /// 
        /// Always call this base function if you override its behavior
        /// </summary>
        public virtual void Update()
        {
            //Update time
            if(_lastUpdateTime == -1.0)
            {
                _currentUpdateTime = Glfw.glfwGetTime();
            }
            _lastUpdateTime = _currentUpdateTime;
            _currentUpdateTime = Glfw.glfwGetTime();

            float timestep = (float) (_currentUpdateTime - _lastUpdateTime);

            //See if we should spawn a new particle
            if( (_currentUpdateTime - _lastSpawnTime) > spawnPeriod )
            {
                this.SpawnParticle();
            }

            //Update all particles


            
            foreach(AParticle particle in _particles)
            {
                particle.Update(timestep);
                if(particle.ShouldDie)
                {
                    _toKill.Add(particle);
                }
            }

            //Destroy any particles that need to be killed
            foreach(AParticle particle in _toKill)
            {
                particle.OnDestroy();
                _particles.Remove(particle);
            }
            _toKill.Clear();

            //Make sure location is accurate
            _transform.TranslationMat(Position.X, Position.Y, Position.Z);
        }

        /// <summary>
        /// Renders the particles of the particle spawner (But not the spawner itself)
        /// </summary>
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

        /// <summary>
        /// Clears all active particles from this spawner
        /// </summary>
        public virtual void DestroyAll()
        {
            _particles.Clear();
        }

        /// <summary>
        /// Creates a particle for this spawner and adds it to the particles for the spawner.
        /// 
        /// Subclasses of AParticleSpawner should override this behavior with the specific
        /// particle spawning behavior wanted, and should call this base function.
        /// </summary>
        protected virtual void SpawnParticle()
        {
            _lastSpawnTime = Glfw.glfwGetTime();
        }
    }
}
