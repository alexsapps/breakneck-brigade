using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breakneck_Brigade
{
    class LocalPlayer : GameObject
    {
        public LocalPlayer(int id)
            : base(id)
        {
            //pos = vector3(0,0,0); 
        }

        public LocalPlayer(int id, string tag)
            : base(id, tag)
        {
            //pos = vector(0,0,0);
        }

        /* public Player(int id, string tag, vector initPos)
          : base(id, tag)
        {

        } */

        public override void Render()
        {
            //draw at position 
            throw new NotImplementedException();
        }

        public override void Update()
        {
            //recieve input and update postion and orientation
            throw new NotImplementedException();
        }
    }
}
