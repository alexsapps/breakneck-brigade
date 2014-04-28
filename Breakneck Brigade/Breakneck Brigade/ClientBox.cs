using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breakneck_Brigade
{
    class ClientBox : ClientGameObject
    {
        public Vector4 c1 { get; set; }
        public Vector4 c2 { get; set; }
        public string Texture { get; set; }

        public override string ModelName
        {
            get { throw new NotImplementedException(); }
        }

        public ClientBox(int id, Vector4 c1, Vector4 c2, string texture, ClientGame game) : base(id, game)
        {
            this.c1 = c1;
            this.c2 = c2;
            this.Texture = texture;
        }

        public ClientBox(int id, BinaryReader reader, ClientGame game) : base (id, game, reader)
        {
            this.c1 = reader.ReadCoordinate();
            this.c2 = reader.ReadCoordinate();
            this.Texture = reader.ReadString();
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
