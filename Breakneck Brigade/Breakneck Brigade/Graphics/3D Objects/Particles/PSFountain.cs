using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breakneck_Brigade.Graphics._3D_Objects.Particles
{
    class PSFountain : AParticleSpawner
    {
        public override void Update()
        {
            base.Update();
            if(_currentNumberOfParticles >= MAX_PARTICLES)
            {
                return;
            }
            else if (_spawning)
            {
                
            }

        }
    }
}
