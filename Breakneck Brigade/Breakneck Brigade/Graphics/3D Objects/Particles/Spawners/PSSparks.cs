using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

namespace Breakneck_Brigade.Graphics
{
    class PSSparks : AParticleSpawner
    {
        Random rand     = new Random();
        //Spawner parameters
        
        /// <summary>
        /// Max velocity in one direction the particles should have
        /// </summary>
        const   double  MAX_VEL         = 50.0;
        /// <summary>
        /// How many particles should be spawned at each spawn event
        /// </summary>
        const   int     PARTS_PER_SPAWN = 20;


        public PSSparks() : base() 
        {
            Lifetime = 2.0f;
        }
        public PSSparks(Vector4 pos) : base(pos) 
        {
            Lifetime = 2.0f;
        }
        public PSSparks(Vector4 pos, ClientGameObject follow)
            : base(pos)
        {
            Lifetime = 0.5f;
            Follow = follow;
            isFollowing = true;
        }

        protected override void Spawn()
        {
 	        base.Spawn();

            for(int ii = 0; ii < PARTS_PER_SPAWN; ii++)
            {
                Vector4 vel = genRandomVelocity();
                Particle3D particle = new Particle3D(Renderer.Textures["colSparkYellow.tga"])
                {
                    Position            = new Vector4(0, 5, 0),
                    Velocity            = vel,
                    Acceleration        = new Vector4(-vel.X * 0.1, -10, -vel.Z * 0.1),
                    Scale               = 0.5f,
                    Rotation            = new Vector4(0, rand.NextDouble()*360.0, 0),
                    RotationalVelocity  = new Vector4(0, 0, 0),
                    Lifetime            = 0.25f
                };

                _particles.Add(particle);
            }
        }
        private Vector4 genRandomVelocity()
        {
            return new Vector4(rand.Next(2) == 0 ? rand.NextDouble() * MAX_VEL : -rand.NextDouble() * MAX_VEL,
                                                rand.NextDouble() * MAX_VEL * 3, //Never negative
                                                rand.Next(2) == 0 ? rand.NextDouble() * MAX_VEL : -rand.NextDouble() * MAX_VEL);
        }
    }
}
