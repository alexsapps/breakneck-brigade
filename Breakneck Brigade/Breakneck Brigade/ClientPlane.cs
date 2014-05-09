using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breakneck_Brigade.Graphics;
using SousChef;

namespace Breakneck_Brigade
{
    class ClientPlane : ClientGameObject
    {
        public string Texture { get; set; }
        public override string ModelName { get { return "floor"; } }

        public ClientPlane(int id, ClientGame game, Vector4 position) : base (id, game, position)
        {
            finilizeConstruction();
        }

        public ClientPlane(int id, BinaryReader reader, ClientGame game) : base (id, game, reader)
        {
            Texture = reader.ReadString();
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
