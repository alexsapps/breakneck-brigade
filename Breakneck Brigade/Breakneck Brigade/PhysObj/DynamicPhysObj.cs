using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breakneck_Brigade
{
    class DynamicPhysObj : PhysObj
    {
        public DynamicPhysObj(int id)
            : base(id)
        {
            //pos = vector(0,0,0)
        }

        public DynamicPhysObj(int id, string tag)
            : base(id, tag)
        {
            //pos = vector(0,0,0)
        }

        /* public StaticPysObj(int id, string tag, vector position)
           {
                //this.pos = pos
           } */

        public override void Update()
        {
            //move 
            throw new NotImplementedException();
        }
    }
}
