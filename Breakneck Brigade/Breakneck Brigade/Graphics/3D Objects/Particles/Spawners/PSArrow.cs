using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using SousChef;

namespace Breakneck_Brigade.Graphics
{
    class PSArrow : AParticleSpawner
    {
        private         float   current     = 0.0f;
        private const   float   DUTY_CYCLE  = 1.0f;
        private         Vector4 DOWN_VEL    = new Vector4(0, -4, 0);
        private         Vector4 UP_VEL      = new Vector4(0, 4, 0);

        public PSArrow() : base() 
        {
            Lifetime = 5f;
            spawnPeriod = Lifetime;
        }
        public PSArrow(Vector4 pos) : base(pos) 
        {
            Lifetime = 5f;
            spawnPeriod = Lifetime;
        }
        
        public override void Render() //Disable transparency and disable face culling
        {
            Renderer.disableTransparency();
            base.Render();
            Renderer.enableTransparency();
        }

        protected override void Spawn()
        {
 	        base.Spawn();
            Particle3D particle = new Particle3D(Renderer.Models["arrow"])
            {
                Position            = new Vector4(0, 5, 0),
                Velocity            = new Vector4(0, 0, 0),
                Acceleration        = new Vector4(0, 0, 0),
                Scale               = 1f,
                Rotation            = new Vector4(0, 0, 0),
                RotationalVelocity  = new Vector4(0, 1, 0),
                Lifetime            = 5f
            };

                _particles.Add(particle);
        }

        public override void Update()
        {
            current += _lastTimeStep;
            current = current % DUTY_CYCLE;

            foreach (AParticle p in _particles)
            {
                p.Velocity = getCurrentVel();
            }

            base.Update();
        }

        private Vector4 getCurrentVel()
        {
            if(current > DUTY_CYCLE/2)
            {
                return DOWN_VEL;
            }
            else if (current > 0)
            {
                return UP_VEL;
            }
            return UP_VEL;
        }
    }
}
