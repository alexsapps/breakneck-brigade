using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

namespace Breakneck_Brigade.Graphics
{
    class PSFlourPoof : AParticleSpawner
    {
                Random rand     = new Random();
        const   double MAX_VEL  = 5.0;

        public PSFlourPoof() : base() 
        { 

        }
        public PSFlourPoof(Vector4 pos) : base(pos) 
        {

        }

        protected override void SpawnParticle()
        {
 	        base.SpawnParticle();

            Vector4 vel = new Vector4(rand.Next(2) == 0 ? rand.NextDouble() * MAX_VEL : -rand.NextDouble() * MAX_VEL,
                                                rand.NextDouble() * MAX_VEL, //Never negative
                                                rand.Next(2) == 0 ? rand.NextDouble() * MAX_VEL : -rand.NextDouble() * MAX_VEL);

            Particle3D flourPoof = new Particle3D(Renderer.Textures["poof.tga"])
            {
                Position            = new Vector4(0, 5, 0),
                Velocity            = vel,
                Acceleration        = new Vector4(-vel.X * 0.01, -2, -vel.Z * 0.01),
                Scale               = 500.0f,
                Rotation            = new Vector4(0, rand.NextDouble()*360.0, 0),
                RotationalVelocity  = new Vector4(0, 0, 0),
                Lifetime            = 1.0f
            };
            _particles.Add(flourPoof);
        }
    }
}
