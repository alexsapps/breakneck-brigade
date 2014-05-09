using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BulletSharp;

namespace DeCuisine
{
    class ServerPlane : ServerGameObject
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Plane; } }
        public string Texture { get; set; }
        public override bool HasBody { get  { return false; } }
        protected override GeometryInfo getGeomInfo() { throw new NotSupportedException(); }
        public override Vector3 Position { get; set; }
        public ServerPlane(ServerGame game, string texture, float height) 
            : base(game)
        {
            Texture = texture;
            Position = new Vector3(0, 0, 0);
            AddToWorld(() =>
            {

                BoxShape groundShape = new BoxShape(500, 10, 500);

                //using motionstate is recommended, it provides interpolation capabilities, and only synchronizes 'active' objects
                DefaultMotionState myMotionState = new DefaultMotionState(Matrix.Identity);
                RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(0, myMotionState, groundShape, Vector3.Zero);
                this.Body = new RigidBody(rbInfo);
                rbInfo.Dispose();

                this.Game.World.AddRigidBody(this.Body);
                return groundShape;
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
