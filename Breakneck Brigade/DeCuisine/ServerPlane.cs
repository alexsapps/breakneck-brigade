using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OdeDotNet;
using OdeDotNet.Geometry;
using OdeDotNet.Joints;

namespace DeCuisine
{
    class ServerPlane : ServerGameObject
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Plane; } }
        public string Texture { get; set; }
        public override bool HasBody { get  { return false; } }
        protected override GeometryInfo getGeomInfo() { throw new NotSupportedException(); }
        public override OdeDotNet.Vector3 Position { get; set; }
        public ServerPlane(ServerGame game, string texture, float height) 
            : base(game)
        {
            Texture = texture;
            Position = new OdeDotNet.Vector3(0, 0, height);
            AddToWorld(() =>
            {
                //dCreatePlane uses plane equation ax+by+cz+d=0.
                //return Ode.dCreatePlane(game.Space, 0, 0, 1.0, 0);
                return this.Game.Space.CreatePlane(0, 0, 1.0f, 0);
            });
        }

        public override void Serialize(System.IO.BinaryWriter stream)
        {
            base.Serialize(stream);
            stream.Write(Texture);
        }

        public override void Update()
        {
           
        }
        public override void OnCollide(ServerGameObject obj)
        {
            // Do NOTHING
        }
    }
}
