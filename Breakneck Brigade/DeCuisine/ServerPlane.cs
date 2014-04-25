using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DeCuisine
{
    class ServerPlane : ServerGameObject
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Plane; } }
        float Height { get; set; }
        public string Texture { get; set; }
        public override bool HasBody { get  { return false; } }
        public override GeometryInfo GeomInfo { get { throw new NotSupportedException(); } }
        public ServerPlane(ServerGame game, string texture, float height) : base(game)
        {
            this.Texture = texture;
            this.Height = height;
        }
        public override void Update()
        {
            
        }
        public override void Serialize(System.IO.BinaryWriter stream)
        {
            base.Serialize(stream);

            stream.Write(Height);
        }
        public override void UpdateStream(System.IO.BinaryWriter stream)
        {
            throw new NotSupportedException();
        }
    }
}
