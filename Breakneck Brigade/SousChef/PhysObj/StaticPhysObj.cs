using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SousChef
{
    class StaticPhysObj : PhysObj
    {
        public StaticPhysObj(int id)
            : base(id)
        {
            //pos = vector(0,0,0)
        }

        public StaticPhysObj(int id, string tag)
            : base(id, tag)
        {

        }

        /* public StaticPysObj(int id, string tag, vector position)
           {
                //this.pos = pos
           } */

        public override void Update()
        {
            //Not moving but may need to interact with still
            throw new NotImplementedException();
        }
    }
}
