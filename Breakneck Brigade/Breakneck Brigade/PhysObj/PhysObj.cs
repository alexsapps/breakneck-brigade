using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breakneck_Brigade
{
    abstract class PhysObj : GameObject
    {
        public PhysObj(int id)
            : base(id)
        {
            //this.pos = vector(0,0,0)
        }

        public PhysObj(int id, string tag) 
            : base(id, tag)
        {
            //this.pos = vector(0,0,0)
        }   

        /* public PysObj(int id, string tag, vector position)
        {
            //this.pos = pos
        } */

        public override void Render()
        {
            throw new NotImplementedException();
        }
    }
}
