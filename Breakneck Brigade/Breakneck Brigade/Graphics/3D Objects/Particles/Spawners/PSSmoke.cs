using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using SousChef;

namespace Breakneck_Brigade.Graphics
{
    class PSSmoke : AParticleSpawner
    {
        Random rand     = new Random();
        List<Texture> smokeTypes;

        /// <summary>
        /// How many particle to spawn per spawn event
        /// </summary>
        const int PARTS_PER_SPAWN = 20;
        /// <summary>
        /// The maximum difference in particle position from the spawner's location
        /// </summary>
        const float MAX_POS = 20f;
        /// <summary>
        /// Maximum time to live on particles
        /// </summary>
        const float MAX_LIFETIME = 7.5f;

        public PSSmoke(SmokeType st) : base() 
        {
            Lifetime = 5f;
            smokeTypes = new List<Texture>();
            populateSmokeTypes(st);
        }
        public PSSmoke(Vector4 pos, SmokeType st) : base(pos) 
        {
            Lifetime = 5f;
            smokeTypes = new List<Texture>();
            populateSmokeTypes(st);
        }
        public PSSmoke(Vector4 pos, SmokeType st, ClientGameObject follow)
        {
            Lifetime = 5f;
            smokeTypes = new List<Texture>();
            populateSmokeTypes(st);
            Follow = follow;
            isFollowing = true;
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

            for(int ii = 0; ii < PARTS_PER_SPAWN; ii++)
            {
                Particle3D particle = new Particle3D(smokeTypes[rand.Next(smokeTypes.Count)])
                {
                    Position            = genRandomPos(),
                    Velocity            = genRandomAcc(),
                    Acceleration        = genRandomAcc(),
                    Scale               = 2f,
                    Rotation            = genRandomRot(),
                    RotationalVelocity  = new Vector4(0,0,0),
                    Lifetime            = (float) (rand.NextDouble() * MAX_LIFETIME)
                };

                _particles.Add(particle);
            }
        }

        private void populateSmokeTypes(SmokeType types)
        {
            if ((int)(types & SmokeType.BLUE) > 0)
                smokeTypes.Add(Renderer.Textures["smokeBlue.tga"]);
            if( (int) (types & SmokeType.GREEN) > 0 )
                smokeTypes.Add(Renderer.Textures["smokeGreen.tga"]);
            if( (int) (types & SmokeType.GREY) > 0 )
                smokeTypes.Add(Renderer.Textures["smokeGrey.tga"]);
            if( (int) (types & SmokeType.RED) > 0 )
                smokeTypes.Add(Renderer.Textures["smokeRed.tga"]);
            if( (int) (types & SmokeType.WHITE) > 0 )
                smokeTypes.Add(Renderer.Textures["smokeWhite.tga"]);
            if( (int) (types & SmokeType.YELLOW) > 0 )
                smokeTypes.Add(Renderer.Textures["smokeYellow.tga"]);
        }

        private Vector4 genRandomPos()
        {
            return new Vector4((rand.Next(2) == 0 ? 1 : -1) * rand.NextDouble() * MAX_POS,
                                0,
                               (rand.Next(2) == 0 ? 1 : -1) * rand.NextDouble() * MAX_POS);
        }
        private Vector4 genRandomAcc()
        {
            return new Vector4(0,
                               rand.NextDouble() * 8 + 6,
                               0);
        }
        private Vector4 genRandomRot()
        {
            return new Vector4((rand.Next(2) == 0 ? 1 : -1) * rand.NextDouble() * 360.0,
                               (rand.Next(2) == 0 ? 1 : -1) * rand.NextDouble() * 360.0,
                               (rand.Next(2) == 0 ? 1 : -1) * rand.NextDouble() * 360.0);
        }
    }
}
