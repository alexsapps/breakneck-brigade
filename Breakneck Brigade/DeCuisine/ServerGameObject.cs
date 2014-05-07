using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using System.IO;
using System.Diagnostics;

using BulletSharp;

namespace DeCuisine
{
    /// <summary>
    /// Top level server game object. All server game objects should inherit. 
    /// </summary> 
    abstract class ServerGameObject : IGameObject
    {
        public ServerGame Game;
        public int Id { get; set; }
        public abstract GameObjectClass ObjectClass { get; }

        public virtual bool HasBody { get { return true; } } //false for walls
        private GeometryInfo _geominfo;
        public GeometryInfo GeomInfo { get { return _geominfo ?? (_geominfo = getGeomInfo()); } } //cache
        protected abstract GeometryInfo getGeomInfo();

        protected SousChef.Vector4 _position;
        public virtual Vector3 Position { get { return getPosition(); } set { setPosition(value); } }
        protected SousChef.Matrix4 _rotation;
        /// <summary>
        /// ODE Matrix (X = left right, Y = in out, Z = up down)
        /// </summary>
        public SousChef.Matrix4 Rotation { get { return getRotation(); } set { setRotation(value); } }

        public bool InWorld { get; protected set; }
        public CollisionShape Geom { get; set; }
        public RigidBody Body { get; set; } //null for walls
        private bool _toRender;
        public bool ToRender { get{return _toRender;} set{_toRender = value; this.MarkDirty();} }
        public bool OnFloor { get; set; }

        private static int nextId;
        private OdeDotNet.Vector3 lastPosition { get; set; }
        /// <summary>
        /// Base constructor. For every servergameobject create their should be a 
        /// coresponding ClientGameObject on each client with the same ID.
        /// </summary>
        public ServerGameObject(ServerGame game) 
        {
            this.Id = nextId++;
            this.Game = game;
            this._rotation = new SousChef.Matrix4();
            this._position = new SousChef.Vector4();
            this.ToRender = true; // class specific implementation can override
            
            Game.ObjectAdded(this);
        }

        /// <summary>
        /// The start the stream packet. Class specific data will always be
        /// written AFTER this data is writen. The packet will look as follows:
        /// int32  id
        /// int16  objectclass
        /// bool   ToRender
        /// double X
        /// double Y
        /// double Z
        /// </summary>
        /// <param name="stream"></param>
        public virtual void Serialize(BinaryWriter stream)
        {
            Game.Lock.AssertHeld();
            serializeEssential(stream);
            stream.Write(this.Position.ToString());
        }

        protected virtual void serializeEssential(BinaryWriter stream)
        {
            stream.Write((Int32)this.Id);
            stream.Write((Int16)this.ObjectClass);
            stream.Write(this.ToRender);
        }

        /// <summary>
        /// Keeps track of the last position and marks dirty if the object 
        /// has moved
        /// </summary>
        public virtual void Update()
        {
            if(Math.Abs(this.lastPosition.X - this.Position.X)  > .01 ||
               Math.Abs(this.lastPosition.Y - this.Position.Y) > .01 ||
               Math.Abs(this.lastPosition.Z - this.Position.Z) > .01){
                   this.MarkDirty(); // it's position moved from the last one
                   this.lastPosition = this.Position;
               }
        }

        protected delegate Geom GeomMaker();

        /// <summary>
        /// Add the object into the physical world.
        /// </summary>
        protected void AddToWorld(Vector3 coordinate)
        {
            AddToWorld(() => { 
                CollisionShape geom = this.MakeGeom(GeomInfo, coordinate);

                if (this.HasBody)
                {
                    Matrix startTransform = new Matrix();
                    DefaultMotionState myMotionState = new DefaultMotionState(startTransform);
                    RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(GeomInfo.Mass, myMotionState, geom, geom.CalculateLocalInertia(GeomInfo.Mass));
                    this.Body = new RigidBody(rbInfo);
                    rbInfo.Dispose();
                }

                //this.Geom.Position = new Vector3(coordinate.X, coordinate.Y, coordinate.Z);
                // Ode.dGeomSetPosition(geom, coordinate.X, coordinate.Y, coordinate.Z); //this must happen after body is set
                return geom;
            });
            
        }

        protected void AddToWorld(GeomMaker geomMaker)
        {
            this.Game.Lock.AssertHeld();
            Debug.Assert(!InWorld);

            this.Geom = geomMaker();
            this.Geom.UserObject = new IntPtr(Id);

            InWorld = true;
        }

        private CollisionShape MakeGeom(GeometryInfo info, Vector3 coordinate)
        {
            CollisionShape geom;
            switch (info.Shape)
            {
                case GeomShape.Box: 
                    //geom = this.Game.Space.CreateBox(info.Sides[0], info.Sides[1], info.Sides[2]);
                    geom = new BoxShape(info.Sides[0], info.Sides[1], info.Sides[2]);
                    break;
                case GeomShape.Sphere: 
                    //geom = this.Game.Space.CreateSphere(info.Sides[0]); 
                    geom = new SphereShape(info.Sides[0]);
                    break;
                default: throw new Exception("AddToWorld not defined for GeomShape of " + info.Shape.ToString());
            }
            return geom;
        }

        private RigidBody makeBody(GeometryInfo info)
        {
            //Mass mass = new Mass();
            //Body body = this.Game.World.CreateBody();
            //mass.SetZero();

            //IntPtr body = Ode.dBodyCreate(this.Game.World);
            switch (info.Shape)
            {
                case GeomShape.Box:
                    //mass.SetBox(info.Mass, info.Sides[0], info.Sides[1], info.Sides[2]);
                    //Ode.dMassSetBox(ref mass, info.Mass, info.Sides[0], info.Sides[1], info.Sides[2]);
                    //Ode.dBodySetMass(body, ref mass);
                    RigidBody

                    break;
                case GeomShape.Sphere:
                    //mass.SetSphere(info.Mass, info.Sides[0]);
                    //Ode.dMassSetSphereTotal(ref mass, GeomInfo.Mass, GeomInfo.Sides[0]);
                    //Ode.dBodySetMass(body, ref mass);
                    break;
                default:
                    throw new Exception("AddToWorld not defined for GeomShape of " + info.Shape.ToString());
            }

            body.Mass = mass;
            return body;
        }


        /// <summary>
        /// Removes the object from the game
        /// </summary>
        public virtual void Remove()
        {
            this.removeFromWorld();        // tell simulation to remove from ode
            this.Game.ObjectRemoved(this); // tell the game to remove from all data structures
        }

        /// <summary>
        /// Remove from the physics world.
        /// </summary>
        private void removeFromWorld()
        {
            this.Game.Lock.AssertHeld();
            Debug.Assert(InWorld);

            if(this.Geom != null)
            {
                this.Geom.Dispose();
            }

            if(this.Body != null)
            {
                this.Body.Dispose();
            }

            this.Geom = null;
            this.Body = null;
            this.InWorld = false;
        }

        /// <summary>
        /// Updates the stream with the position. Reads all of the positions from the packet
        /// The packet will should look as follows
        /// int32  id
        /// bool   ToRender
        /// double x
        /// double y
        /// double z
        /// </summary>
        /// <param name="stream"></param>
        public virtual void UpdateStream(BinaryWriter stream)
        {
            stream.Write((Int32)Id);
            stream.Write((bool)ToRender); // no need to cast but being explicit gets my jollys off
            stream.Write(this.Position.ToString());
        }

        public virtual void OnCollide(ServerGameObject obj)
        {

        }

        public void MarkDirty()
        {
            this.Game.ObjectChanged(this);
        }

        public void MarkDeleted()
        {
            this.Remove();
        }

        private OdeDotNet.Vector3 getPosition()
        {
            Game.Lock.AssertHeld();
            if (this.Geom != null)
            {
                OdeDotNet.Vector3 m3 = this.Geom.Position;
                _position.Set(m3.X, m3.Y, m3.Z);
                return m3;
            }
            else
            {
                throw new Exception("Geom is null");
            }
        }

        private void setPosition(OdeDotNet.Vector3 value)
        {
            this.Game.Lock.AssertHeld();
            this.MarkDirty(); // moved it, make sure you mark it to move
            this.lastPosition = value;
            this.Geom.Position = new OdeDotNet.Vector3(value.X, value.Y, value.Z); 
        }

        private SousChef.Matrix4 getRotation()
        {
            if(this.Geom != null)
            {
                OdeDotNet.Matrix3 r = this.Geom.Rotation;
                _rotation.SetAll(r.M00,  r.M10,  r.M20,  0,
                                 r.M01,  r.M11,  r.M21,  0,
                                 r.M02,  r.M12,  r.M22,  0,
                                 0,      0,      0,      1);
                return _rotation;
            }
            else
            {
                throw new Exception("Geom is null");
            }
        }
        private void setRotation(SousChef.Matrix4 rotation)
        {
            if (this.Geom != null)
            {
                double[] arr = {_rotation[0,0], _rotation[1,0], _rotation[2,0],
                                _rotation[0,1], _rotation[1,1], _rotation[2,1],
                                _rotation[0,2], _rotation[1,2], _rotation[2,2]};
                OdeDotNet.Matrix3 r = new OdeDotNet.Matrix3();
                r.SetIdentity();
                r.M00 = _rotation[0, 0];
                r.M10 = _rotation[1, 0];
                r.M20 = _rotation[2, 0];
                r.M01 = _rotation[0, 1]; 
                r.M11 = _rotation[1, 1]; 
                r.M21 = _rotation[2, 1];
                r.M02 = _rotation[0, 2]; 
                r.M12 = _rotation[1, 2];
                r.M22 = _rotation[2, 2];
            }
            else
            {
                throw new Exception("Geom is null");
            }
        }
    }
}
