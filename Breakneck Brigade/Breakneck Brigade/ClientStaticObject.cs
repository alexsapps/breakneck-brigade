using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

namespace Breakneck_Brigade
{
    class ClientStaticObject : ClientGameObject
    {
        private string _modelName;
        public override string ModelName
        {
            get { return _modelName; }
        }
        public ClientStaticObject(int id,  ClientGame game) : base(id, game)
        {
        }

        public ClientStaticObject(int id, BinaryReader reader, ClientGame game) : base (id, game, reader)
        {
            this._modelName = reader.ReadString();
            base.finalizeConstruction();
        }

        public override void StreamUpdate(BinaryReader reader)
        {
            base.StreamUpdate(reader);
        }

    }
}
