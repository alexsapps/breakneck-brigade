using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Tao.OpenGl;

namespace Breakneck_Brigade.Graphics
{
    class PSStars : AParticleSpawner
    {
        Random rand     = new Random();
        //Spawner parameters
        
        /// <summary>
        /// Max velocity in one direction the particles should have
        /// </summary>
        const   double  MAX_VEL         = 10.0;
        /// <summary>
        /// How many particles should be spawned at each spawn event
        /// </summary>
        const   int     PARTS_PER_SPAWN = 1;
        /// <summary>
        /// The range of rotation speed (from BASE to BASE + ROT_VEL_RANGE)
        /// </summary>
        const   double  ROT_VEL_RANGE = 5;
        /// <summary>
        /// The slowest the particle should spin
        /// </summary>
        const   double BASE_ROT_VEL = 5;

        const   float  PARTICLE_LIFETIME = 2.0f;




        public PSStars() : base() 
        {
            Lifetime = -1f;
            this.spawnPeriod = .05;
        }
        public PSStars(Vector4 pos) : base(pos) 
        {
            Lifetime = -1f;
            this.spawnPeriod = .05;
        }
        public PSStars(Vector4 pos, ClientGameObject follow)
            : base(pos)
        {
            Lifetime = -1f;
            Follow = follow;
            isFollowing = true;
            this.spawnPeriod = .05;
        }

        protected override void Spawn()
        {
 	        base.Spawn();

            for(int ii = 0; ii < PARTS_PER_SPAWN; ii++)
            {
                Vector4 vel = genRandomVelocity();
                Particle3D particle = new Particle3D(Renderer.Models["star"])
                {
                    Position            = new Vector4(0, 0, 0),
                    Velocity            = vel,
                    Acceleration        = new Vector4(-vel.X * 0.1, -10, -vel.Z * 0.1),
                    Scale               = 0.1f,
                    Rotation            = new Vector4(0, rand.NextDouble()*360.0, 0),
                    RotationalVelocity  = genRandomRotVel(),
                    Lifetime            = PARTICLE_LIFETIME
                };

                _particles.Add(particle);
            }
        }

        public override void Render() //Disable transparency and disable face culling
        {
            Renderer.disableTransparency();
            Gl.glDisable(Gl.GL_CULL_FACE);
            base.Render();
            Gl.glEnable(Gl.GL_CULL_FACE);
            Renderer.enableTransparency();
        }
        private Vector4 genRandomVelocity()
        {
            return new Vector4((rand.Next(2) == 0 ? rand.NextDouble() : -rand.NextDouble()) * MAX_VEL * 2,
                               rand.NextDouble() * MAX_VEL * 2, //Never negative
                               (rand.Next(2) == 0 ? rand.NextDouble() : -rand.NextDouble()) * MAX_VEL * 2);
        }
        private Vector4 genRandomRotVel()
        {
            return new Vector4(0,
                               ((rand.Next(2) == 0 ? rand.NextDouble() : -rand.NextDouble()) * ROT_VEL_RANGE) + BASE_ROT_VEL,
                               0);
        }
    }
}
