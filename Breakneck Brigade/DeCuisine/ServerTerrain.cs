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
        public TerrainType Type;
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Terrain; } }
        
        public override bool HasBody { get  { return false; } }

        protected GeometryInfo _geomInfo;
        protected override GeometryInfo getGeomInfo() { return _geomInfo; }

        public override int SortOrder { get { return 0; } }

        /// <summary>
        /// Create intial terrtain
        /// </summary>
        /// <param name="game"></param>
        /// <param name="texture"></param>
        /// <param name="info"></param>
        public ServerTerrain(ServerGame game, TerrainType type, Vector3 position, GeometryInfo info) 
            : base(game)
        {
            this.Type = type;
            this._geomInfo = info;

            AddToWorld(() =>
            {
                CollisionShape groundShape;
                var sides = GeomInfo.Size;
                switch (this.GeomInfo.Shape)
                {
                    case GeomShape.Box:
                        groundShape = new BoxShape(sides[0], sides[1], sides[2]);
                        break;
                    case GeomShape.Cylinder:
                        groundShape = new CylinderShape(sides[0], sides[1], sides[2]);
                        break;
                    default:
                        throw new Exception("bad shape for ServerTerrain");
                }

                //using motionstate is recommended, it provides interpolation capabilities, and only synchronizes 'active' objects
                DefaultMotionState myMotionState = new DefaultMotionState
                    (
                        Matrix.RotationYawPitchRoll(this.GeomInfo.Euler.X, this.GeomInfo.Euler.Y, this.GeomInfo.Euler.Z) * Matrix.Translation(position)
                    );

                using (var rbInfo = new RigidBodyConstructionInfo(0, myMotionState, groundShape, Vector3.Zero))
                {
                    this.Body = new RigidBody(rbInfo);
                }

                this.Body.ProceedToTransform(this.Body.WorldTransform * Matrix.RotationY(this.GeomInfo.Orientation));

                this.Game.World.AddRigidBody(this.Body);

                this._modelScale = Matrix4.MakeScalingMat(this.GeomInfo.Size[0] / this.GeomInfo.ModelScale[0],
                              this.GeomInfo.Size[1] / this.GeomInfo.ModelScale[1],
                              this.GeomInfo.Size[2] / this.GeomInfo.ModelScale[2]);

                return groundShape;
            });
        }

        public override void Serialize(System.IO.BinaryWriter stream)
        {
            base.Serialize(stream);
            stream.Write(Type.Name);
            stream.Write(GeomInfo.Size[0]);
            stream.Write(GeomInfo.Size[1]);
            stream.Write(GeomInfo.Size[2]);
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
