using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Breakneck_Brigade.Graphics;
using System.IO;

namespace Breakneck_Brigade
{
    class ClientCooker : ClientGameObject
    {
        public ClientCooker(int id, Vector4 transform, ClientGame game)
            : base(id, new Vector4(), game)
        {
            construct();
        }

        //called by ClientGameObject.Deserialize
        public ClientCooker(int id, BinaryReader reader, ClientGame game) : base(id, reader, game)
        {
            construct();
        }

        private void construct()
        {
            update();
        }

        private void update()
        {

        }

        public override void Update(BinaryReader reader)
        {
            base.Update(reader);

            update();
        }

        public override void Render()
        {
            throw new NotImplementedException();
        }
    }

}
