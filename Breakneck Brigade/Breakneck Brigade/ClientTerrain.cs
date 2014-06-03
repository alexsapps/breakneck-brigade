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

        private float[] _sides;
        public override float[] Sides { get { return _sides; } }

        public ClientTerrain(int id, ClientGame game, Vector4 position, float[] sides) : base (id, game, position)
        {
            finalizeConstruction();
            this._sides = sides;
        }

        public ClientTerrain(int id, BinaryReader reader, ClientGame game) : base (id, game, reader)
        {
            _modelName = reader.ReadString();
            //TODO:  this.Type = game.Config.Terrain[ingrname];
            _sides = new float[] { reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle() };
            finalizeConstruction();
        }
        
    }
}
