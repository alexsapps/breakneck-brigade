using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using BulletSharp;

namespace DeCuisine
{
    class ServerTerrain : ServerGameObject
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Plane; } }
        public string Texture { get; set; }
        public override bool HasBody { get  { return false; } }
        protected override GeometryInfo getGeomInfo() { return new GeometryInfo(); }
        public override Vector3 Position { get; set; }
        public ServerTerrain(ServerGame game, string texture, float height) 
            : base(game)
        {
            this.Texture = texture;
            this.Position = this.GeomInfo.Position;
            AddToWorld(() =>
            {
                CollisionShape groundShape;
                switch (this.GeomInfo.Shape)
                {
                    case GeomShape.Box:
                        groundShape = new BoxShape(this.GeomInfo.Sides[0], this.GeomInfo.Sides[1], this.GeomInfo.Sides[2]);
                        break;
                    case GeomShape.Cylinder:
                        groundShape = new CylinderShape(this.GeomInfo.Sides[0], this.GeomInfo.Sides[1], this.GeomInfo.Sides[2]);
                        break;
                    default:
                        groundShape = new BoxShape(500, 10, 500);
                        break;
                }

                //using motionstate is recommended, it provides interpolation capabilities, and only synchronizes 'active' objects
                DefaultMotionState myMotionState = new DefaultMotionState
                    (
                        Matrix.RotationYawPitchRoll(this.GeomInfo.Euler.X, this.GeomInfo.Euler.Y, this.GeomInfo.Euler.Z) * Matrix.Translation(this.GeomInfo.Position)
                    );
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
