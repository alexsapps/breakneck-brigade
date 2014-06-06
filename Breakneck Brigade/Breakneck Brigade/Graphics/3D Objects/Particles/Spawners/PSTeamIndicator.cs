using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using SousChef;

namespace Breakneck_Brigade.Graphics
{
    class PSTeamIndicator : AParticleSpawner
    {
        ClientGameObject Follow = null;
        static float playerHeight = BB.GetPlayerSides()[1];
        string team;

        /// <summary>
        /// THIS IS A CLIENT SIDE PARTICLE EFFECT DO NOT USE SERVERSIDE
        /// </summary>
        public PSTeamIndicator(ClientPlayer cp) : base() 
        {
            Lifetime = -1f;
            Follow = cp;
            team = cp.TeamName;
            StartSpawning();
        }

        public override void StartSpawning()
        {
            base.StartSpawning();
            this.Spawn();
            this.StopSpawning();
        }

        public override void Render() //Disable transparency and disable face culling
        {
            Renderer.disableTransparency();
            if(Follow != Program.localPlayer.Player)
                base.Render();
            Renderer.enableTransparency();
        }

        protected override void Spawn()
        {
 	        base.Spawn();
            Particle3D particle = new Particle3D(Renderer.Models["indicator"], team == "red" ? Renderer.Textures["confettiRed.tga"] : Renderer.Textures["confettiBlue.tga"])
            {
                Position            = new Vector4(0, playerHeight/2 + 15, 0),
                Velocity            = new Vector4(0, 0, 0),
                Acceleration        = new Vector4(0, 0, 0),
                Scale               = 1.0f,
                Rotation            = new Vector4(0, 0, 0),
                RotationalVelocity  = new Vector4(0, 0.5, 0),
                Lifetime            = -1
            };

                _particles.Add(particle);
        }

        public override void Update()
        {
            Position = Follow.Position;

            base.Update();
        }
    }
}
