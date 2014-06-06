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
        protected       float           _lastTimeStep;
        protected       bool            _spawning;
        /// <summary>
        /// The maximum amount of particles this spawner should have out at any time
        /// </summary>
        public          int             MaxParticles = 99;
        /// <summary>
        /// How often the particles should spawn in seconds
        /// </summary>
        public          double          spawnPeriod = .1000;

        /// <summary>
        /// How long this spawner should be active for
        /// If lifetime is = -1, the spawner will run forever
        /// By default, lifetime is -1
        /// </summary>
        public float Lifetime = -1;
        /// <summary>
        /// How long this spawner has currently be running for
        /// </summary>
        protected float _age { get; set; }

        /// <summary>
        /// A client object to follows
        /// </summary>
        public ClientGameObject Follow = null; 

        public bool RemoveMe { get; protected set; }

        public AParticleSpawner()
        {
            _particles                  = new List<AParticle>();
            _toKill                     = new List<AParticle>();
            _lastUpdateTime             = -1.0;
            _transform                  = new Matrix4();
            _spawning                   = false;
            Position                    = new Vector4();
            RemoveMe                    = false;
        }

        public AParticleSpawner(Vector4 position)
        {
            _particles                  = new List<AParticle>();
            _toKill                     = new List<AParticle>();
            _lastUpdateTime             = -1.0;
            _transform                  = new Matrix4();
            _spawning                   = false;
            Position                    = position;
            RemoveMe                    = false;
        }

        /// <summary>
        /// Turns on spawning. Can be used to run additional logic
        /// </summary>
        public virtual void StartSpawning()
        {
            _spawning = true;
            _age = 0.0f;
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

            _lastTimeStep = (float) (_currentUpdateTime - _lastUpdateTime);

            if (Lifetime != -1)
            {
                _age += _lastTimeStep;
                if (_age > Lifetime)
                {
                    if(_spawning)
                        StopSpawning();
                    if(_particles.Count == 0)
                        RemoveMe = true;
                }
            }

            //See if we should spawn a new particle
            if( (_currentUpdateTime - _lastSpawnTime) > spawnPeriod  && _spawning)
            {
                this.Spawn();
            }

            //Update all particles
            foreach(AParticle particle in _particles)
            {
                particle.Update(_lastTimeStep);
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
            if(_toKill.Count > 0)
                _toKill.Clear();

            if (Follow != null)
            {
                Position = Follow.Position;
            }

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
        protected virtual void Spawn()
        {
            _lastSpawnTime = Glfw.glfwGetTime();
        }
    }
}
