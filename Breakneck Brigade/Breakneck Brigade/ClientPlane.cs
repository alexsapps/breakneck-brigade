using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breakneck_Brigade.Graphics;

namespace Breakneck_Brigade
{
    class ClientPlane : ClientGameObject
    {
        public override string ModelName { get { return "#plane"; } }

        public ClientPlane(int id, ClientGame game) : base (id, game)
        {
            finilizeConstruction();
        }

        public ClientPlane(int id, BinaryReader reader, ClientGame game) : base (id, game, reader)
        {
            finilizeConstruction();
        }
        
        public override void StreamUpdate(BinaryReader reader)
        {
            throw new NotSupportedException();
        }

        public override void Render()
        {
            base.Render();
        }
    }
}
