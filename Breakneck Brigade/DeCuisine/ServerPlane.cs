using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tao.Ode;

namespace DeCuisine
{
    class ServerPlane : ServerGameObject
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Plane; } }
        float Height { get; set; }
        public string Texture { get; set; }
        public override bool HasBody { get  { return false; } }
        protected override GeometryInfo getGeomInfo() { throw new NotSupportedException(); }
        public override Ode.dVector3 Position { get { throw new NotSupportedException(); } set { throw new NotSupportedException(); } }
        public ServerPlane(ServerGame game, string texture, float height) : base(game)
        {
            this.Texture = texture;
            this.Height = height;
            AddToWorld(() =>
            {
                return Ode.dCreatePlane(game.Space, 0, 0, height, 0);
            });
        }
        public override void Update()
        {
            
        }
        public override void Serialize(System.IO.BinaryWriter stream)
        {
            //do not call base serialize
            serializeEssential(stream);
            stream.Write(Height);
            stream.Write(Texture);
        }
        public override void UpdateStream(System.IO.BinaryWriter stream)
        {
            throw new NotSupportedException();
        }
    }
}
