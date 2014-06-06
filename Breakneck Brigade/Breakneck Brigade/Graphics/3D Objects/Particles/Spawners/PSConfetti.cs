using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using SousChef;

namespace Breakneck_Brigade.Graphics
{
    class PSConfetti : AParticleSpawner
    {
        Random rand                 = new Random();

        /// <summary>
        /// How many particle to spawn per spawn event
        /// </summary>
        const int PARTS_PER_SPAWN = 20;
        /// <summary>
        /// The maximum difference in particle position
        /// </summary>
        const float MAX_POS = 10f;
        const float MAX_ROT_VEL = 3f;

        public PSConfetti() : base() 
        {
            Lifetime = 5f;
        }
        public PSConfetti(Vector4 pos) : base(pos) 
        {
            Lifetime = 5f;
        }
        public PSConfetti(Vector4 pos, ClientGameObject follow)
            : base(pos)
        {
            Lifetime    = 5f;
            Follow      = follow;
        }

        public override void StartSpawning()
        {
            base.StartSpawning();
            base.DestroyAll();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Render() //Disable transparency and disable face culling
        {
            Renderer.disableTransparency();
            Gl.glDisable(Gl.GL_CULL_FACE);
            base.Render();
            Gl.glEnable(Gl.GL_CULL_FACE);
            Renderer.enableTransparency();
        }

        protected override void Spawn()
        {
 	        base.Spawn();

            for(int ii = 0; ii < PARTS_PER_SPAWN; ii++)
            {
                Texture tex;
                switch(rand.Next(3))
                {
                    case 0:
                        tex = Renderer.Textures["confettiRed.tga"];
                        break;
                    case 1:
                        tex = Renderer.Textures["confettiBlue.tga"];
                        break;
                    case 2:
                        tex = Renderer.Textures["confettiGreen.tga"];
                        break;
                    default:
                        tex = Renderer.Textures["confettiRed.tga"];
                        break;
                }

                Particle2D particle = new Particle2D(tex)
                {
                    Position            = genRandomPos(),
                    Velocity            = new Vector4(0, 0, 0),
                    Acceleration        = new Vector4(0, -8, 0),
                    Scale               = 50f,
                    Rotation            = genRandomRot(),
                    RotationalVelocity  = genRandomRot(),
                    Lifetime            = 4.0f
                };

                _particles.Add(particle);
            }
        }

        private Vector4 genRandomPos()
        {
            return new Vector4((rand.Next(2) == 0 ? 1 : -1) * rand.NextDouble() * MAX_POS,
                            (rand.Next(2) == 0 ? -1 : -2) * rand.NextDouble() * MAX_POS + 20,
                            (rand.Next(2) == 0 ? 1 : -1) * rand.NextDouble() * MAX_POS);
        }

        private Vector4 genRandomRot()
        {
            return new Vector4((rand.Next(2) == 0 ? 1 : -1) * rand.NextDouble() * MAX_ROT_VEL,
                               (rand.Next(2) == 0 ? 1 : -1) * rand.NextDouble() * MAX_ROT_VEL,
                               (rand.Next(2) == 0 ? 1 : -1) * rand.NextDouble() * MAX_ROT_VEL);
        }
    }
}
