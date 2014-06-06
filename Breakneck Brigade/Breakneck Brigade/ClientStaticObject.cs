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
        public override string ModelName { get { return _modelName; } }
        public string FriendlyName { get; set; }
        float[] _sides;
        public override float[] Sides { get { return _sides; } }
        public string Team { get; set; }
        
        public ClientStaticObject(int id,  ClientGame game) : base(id, game)
        {
        }

        public ClientStaticObject(int id, BinaryReader reader, ClientGame game) : base (id, game, reader)
        {
            this._modelName = reader.ReadString();
            this.FriendlyName = reader.ReadString();
            this.Team = reader.ReadString();
            this._sides = new float[reader.ReadInt32()];
            for (int i = 0; i < this._sides.Length; i++)
                this._sides[i] = reader.ReadSingle();
            base.finalizeConstruction();
        }

        public override string ToString()
        {
            return FriendlyName;
        }
        public override void Render()
        {
            if (this.Team == "red")
                base.RenderColored(1f, 0.5f, .5f);
            else if (this.Team == "blue")
                base.RenderColored(.5f, .5f, 1f);
            else if (this._modelName == "cdj" && this.Game.LookatId == this.Id)
               base.RenderColored(0.0f, 1f, 0.0f);
            else
                base.Render();
        }
    }
}
