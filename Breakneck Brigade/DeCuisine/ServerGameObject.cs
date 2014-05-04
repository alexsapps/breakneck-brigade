using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using System.IO;
using System.Diagnostics;
using Tao.Ode;

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
        public virtual Ode.dVector3 Position { get { return getPosition(); } set { setPosition(value); } }

        public bool InWorld { get; protected set; }
        public IntPtr Geom { get; set; }
        public IntPtr Body { get; set; } //null for walls
        private bool _toRender;
        public bool ToRender { get{return _toRender;} set{_toRender = value; this.MarkDirty();} }
        public bool OnFloor { get; set; }

        private static int nextId;
        private Ode.dVector3 lastPosition { get; set; }
        /// <summary>
        /// Base constructor. For every servergameobject create their should be a 
        /// coresponding ClientGameObject on each client with the same ID.
        /// </summary>
        public ServerGameObject(ServerGame game) 
        {
            this.Id = nextId++;
            this.Game = game;
            this.ToRender = true; // class specific implementation can override
            
            game.Lock.AssertHeld();
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
            serializeEssential(stream);
            stream.Write(this.Position);
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

        /// <summary>
        /// Removes the object from the game
        /// </summary>
        public virtual void Remove()
        {
            this.RemoveFromWorld();        // tell simulation to remove from ode
            this.Game.ObjectRemoved(this); // tell the game to remove from all data structures
        }

        protected delegate IntPtr GeomMaker();

        /// <summary>
        /// Add the object into the physical world.
        /// </summary>
        protected void AddToWorld(Ode.dVector3 coordinate)
        {
            AddToWorld(() => { 
                
                var geom = makeGeom(GeomInfo, coordinate);

                if (this.HasBody)
                {
                    Body = makeBody(GeomInfo, Geom);
                    Ode.dGeomSetBody(geom, Body);
                }

                Ode.dGeomSetPosition(geom, coordinate.X, coordinate.Y, coordinate.Z); //this must happen after body is set

                return geom;

            });
            
        }

        protected void AddToWorld(GeomMaker geomMaker)
        {
            Debug.Assert(!InWorld);

            Geom = geomMaker();
            Ode.dGeomSetData(Geom, new IntPtr(Id));

            InWorld = true;
        }

        private IntPtr makeGeom(GeometryInfo info, Ode.dVector3 coordinate)
        {
            IntPtr geom;
            switch (info.Shape)
            {
                case GeomShape.Box: geom = Ode.dCreateBox(Game.Space, info.Sides[0], info.Sides[1], info.Sides[2]); break;
                case GeomShape.Sphere: geom = Ode.dCreateSphere(Game.Space, info.Sides[0]); break;
                default: throw new Exception("AddToWorld not defined for GeomShape of " + info.Shape.ToString());
            }
            return geom;
        }

        private IntPtr makeBody(GeometryInfo info, IntPtr geom)
        {
            Ode.dMass mass = new Ode.dMass();
            IntPtr body = Ode.dBodyCreate(this.Game.World);
            switch (info.Shape)
            {
                case GeomShape.Box:
                    Ode.dMassSetBox(ref mass, info.Mass, info.Sides[0], info.Sides[1], info.Sides[2]);
                    Ode.dBodySetMass(body, ref mass);
                    break;
                case GeomShape.Sphere:
                    Ode.dMassSetZero(ref mass);
                    Ode.dMassSetSphereTotal(ref mass, GeomInfo.Mass, GeomInfo.Sides[0]);
                    Ode.dBodySetMass(body, ref mass);
                    break;
                default:
                    throw new Exception("AddToWorld not defined for GeomShape of " + GeomInfo.Shape.ToString());
            }
            return body;
        }

        public void RemoveFromWorld()
        {
            Game.Lock.AssertHeld();
            Debug.Assert(InWorld);

            if(this.Geom != IntPtr.Zero)
            {
                Ode.dGeomDestroy(this.Geom);
            }

            if(this.Body != IntPtr.Zero)
            {
                Ode.dBodyDestroy(this.Body);
            }

            this.Geom = IntPtr.Zero;
            this.Body = IntPtr.Zero;
            InWorld = false;
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
            stream.Write(this.Position);
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

        private Ode.dVector3 getPosition()
        {
            if (this.Geom != IntPtr.Zero)
            {
                Ode.dVector3 m3 = Ode.dGeomGetPosition(this.Geom);
                return m3;
            }
            else
            {
                throw new Exception("Geom is null");
            }
        }

        private void setPosition(Ode.dVector3 value)
        {
            this.MarkDirty(); // moved it, make sure you mark it to move
            this.lastPosition = value;
            Ode.dGeomSetPosition(this.Geom, value.X, value.Y, value.Z); 
        }

    }
}
