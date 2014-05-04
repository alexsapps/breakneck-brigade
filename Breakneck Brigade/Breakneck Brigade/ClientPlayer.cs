using SousChef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breakneck_Brigade
{
    class ClientPlayer : ClientGameObject
    {
        public override string ModelName { get { return "player"; } }

        public ClientPlayer(int id, Vector4 position, ClientGame game)
            : base(id, game, position)
        {
            //propertyA = parameterA;
            //propertyB = parameterB;
            //propertyC = parameterC;
            //propertyD = parameterD;
            base.finilizeConstruction();
        }

        public ClientPlayer(int id, BinaryReader reader, ClientGame game)
            : base(id, game, reader)
        {
            //propertyA = reader.readString()
            //propertyB = reader.readString()
            //propertyC = reader.readString()
            //propertyD = reader.readString()
            base.finilizeConstruction();
        }

        public void Update(Vector4 position)
        {
            base.BaseUpdate(position);
            //propertyC = parameterC;
            //propertyD = parameterD;
        }

        public override void StreamUpdate(BinaryReader reader)
        {
            base.StreamUpdate(reader);
            //propertyC = reader.ReadString();
            //propertyD = reader.ReadString();
        }
    }
}
