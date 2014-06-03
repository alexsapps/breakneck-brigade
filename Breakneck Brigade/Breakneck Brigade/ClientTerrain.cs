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
    class ClientTerrain : ClientGameObject
    {
        public string Texture { get; set; }
        private string _modelName;
        public override string ModelName { get { return _modelName; } }
        public TerrainType Type;

        private float[] _sides;
        public override float[] Sides { get { return _sides; } }

        public ClientTerrain(int id, ClientGame game, Vector4 position, float[] sides) : base (id, game, position)
        {
            finalizeConstruction();
            this._sides = sides;
        }

        public ClientTerrain(int id, BinaryReader reader, ClientGame game) : base (id, game, reader)
        {
            string terrainName = reader.ReadString();
            Type = game.Config.Terrains[terrainName];
            _modelName = terrainName;
            _sides = new float[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() };
            finalizeConstruction();
        }
        
        public override void StreamUpdate(BinaryReader reader)
        {
            throw new NotSupportedException();
        }

        public override string ToString()
        {
            return Type.FriendlyName;
        }
    }
}
