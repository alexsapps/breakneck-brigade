using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SousChef;

namespace DeCuisine
{
    class ServerBox : ServerGameObject
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Box; } }
        public Coordinate c1 { get; set; }
        public Coordinate c2 { get; set; }
        public string Texture { get; set; }
        public override bool HasBody { get { return false; } }
        public override GeometryInfo GeomInfo { get { throw new NotSupportedException(); } }

        public ServerBox(ServerGame game, string texture, Coordinate c1, Coordinate c2) : base(game)
        {
            this.Texture = texture;
            this.c1 = c1;
            this.c2 = c2;
        }
        public override void Update()
        {

        }
        public override void Serialize(System.IO.BinaryWriter stream)
        {
            base.Serialize(stream);
        }
        public override void UpdateStream(System.IO.BinaryWriter stream)
        {
            throw new NotSupportedException();
        }
    }
}
