using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SousChef
{
    class ExternalPlayer : GameObject
    {
        public ExternalPlayer(int id)
            : base(id)
        {
            //this.pos = vector(0,0,0)
        }


        /* public ExternalPlayer(int id, string tag, vector position)
        {
            //this.pos = pos
        } */
        
        public override void Render()
        {
            throw new NotImplementedException();
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }
    }
}
