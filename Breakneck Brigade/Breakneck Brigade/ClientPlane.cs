using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breakneck_Brigade
{
    class ClientPlane : ClientGameObject
    {
        public float Height { get; set; }
        public string Texture { get; set; }
        public override string ModelName
        {
            get { return Texture; }
        }

        public ClientPlane(int id, int height, string texture, ClientGame game) : base (id, game)
        {
            this.Height = height;
            this.Texture = texture;
            base.finilizeConstruction();
        }

        public ClientPlane(int id, BinaryReader reader, ClientGame game) : base (id, game, reader)
        {
            this.Height = reader.ReadSingle();
            this.Texture = reader.ReadString();
            base.finilizeConstruction();
        }

        protected override void readGeom(BinaryReader reader)
        {
            //don't read coordinates, do not call baes.readGeom()
        }

        public override void StreamUpdate(BinaryReader reader)
        {
            throw new NotSupportedException();
        }
    }
}
