using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SousChef;

using BulletSharp;

namespace DeCuisine
{
    class ServerStaticObject : ServerGameObject
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Static; } }
        public string Model { get; set; } // don't need to render the model, but a convinient way to store type
        protected override GeometryInfo getGeomInfo() { return BBXItemParser.getGeomInfo(); }

        public ServerStaticObject(ServerGame game, string model, Vector3 position)
            : base(game)
        {
            this.Model = model;
            this.Position = position;
            base.AddToWorld(this.Position);
        }
        public override void Update()
        {
            
        }
        public override void Serialize(System.IO.BinaryWriter stream)
        {
            base.Serialize(stream);
            stream.Write(this.model);
        }
        public override void UpdateStream(System.IO.BinaryWriter stream)
        {
            base.UpdateStream(stream);
        }
    }
}
