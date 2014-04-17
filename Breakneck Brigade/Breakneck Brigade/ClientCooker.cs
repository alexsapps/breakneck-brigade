using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Breakneck_Brigade.Graphics;

namespace Breakneck_Brigade
{
    class ClientCooker : ClientGameObject
    {
        public ClientCooker(int id, Vector4 transform, Client client, Model model)
            : base(id, new Vector4(), client, model)
        {
            
        }

        public override void Render()
        {
            throw new NotImplementedException();
        }
    }

}
