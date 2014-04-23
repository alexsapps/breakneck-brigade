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
        public int Id { get; set; }
        public abstract GameObjectClass ObjectClass { get; }

        public bool InWorld { get; protected set; }

        public IntPtr Geom { get; set; }
        public virtual bool HasBody { get { return true; } } //false for walls
        public IntPtr Body { get; set; } //null for walls
        public GeomShape GeomShape { get; set; }

        public abstract GeometryInfo GeomInfo { get; }

        public ServerGame Game;

        /// <summary>
        /// Base constructor. For every servergameobject create their should be a 
        /// coresponding ClientGameObject on each client with the same ID.
        /// </summary>
        public ServerGameObject(int id, ServerGame game) 
        {
            this.Id = id;
            this.Game = game;

            game.Lock.AssertHeld();
            Game.ObjectAdded(this);
        }

        /// <summary>
        /// Specific to each subclass. 
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Removes the object from the game
        /// </summary>
        public virtual void Remove()
        {
            this.RemoveFromWorld();
            this.Game.ObjectRemoved(this);

            // Ode.dVector3 m3 = Ode.dGeomGetPosition(this.Geom);
            // Ode.dGeomGetRotation(this.Geom);
        }

        /// <summary>
        /// Add the object into the physical world.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public void AddToWorld(float x, float y, float z)
        {
            Debug.Assert(!InWorld);

            switch (GeomInfo.Shape)
            {
                case GeomShape.Box:
                    this.Geom = Ode.dCreateBox(this.Game.Space, GeomInfo.Sides[0], GeomInfo.Sides[1], GeomInfo.Sides[2]);
                    break;
                case GeomShape.Sphere:
                    Geom = Ode.dCreateSphere(this.Game.Space, GeomInfo.Sides[0]);
                    break;
                default:
                    throw new Exception("AddToWorld not defined for GeomShape of " + GeomInfo.Shape.ToString());
            }

            Ode.dGeomSetPosition(this.Body, x, y, z);
            if (this.HasBody)
            {
                Ode.dMass mass = new Ode.dMass();
                this.Body = Ode.dBodyCreate(this.Game.World);
                switch (GeomInfo.Shape)
                {
                    case GeomShape.Box:
                        Ode.dMassSetBox(ref mass, GeomInfo.Mass, GeomInfo.Sides[0], GeomInfo.Sides[1], GeomInfo.Sides[2]);
                        Ode.dBodySetMass(this.Body, ref mass);
                        break;
                    case GeomShape.Sphere:
                        Ode.dMassSetZero(ref mass);
                        Ode.dMassSetSphereTotal(ref mass, GeomInfo.Mass, GeomInfo.Sides[0]);
                        Ode.dBodySetMass(this.Body, ref mass);
                        this.Geom = Ode.dCreateSphere(this.Game.Space, GeomInfo.Sides[0]);
                        break;
                    default:
                        throw new Exception("AddToWorld not defined for GeomShape of " + GeomInfo.Shape.ToString());
                }

                Ode.dGeomSetBody(this.Geom, this.Body);
            }

            Ode.dGeomSetData(Geom, new IntPtr(Id));
            InWorld = true;
        }

        public void RemoveFromWorld()
        {
            Debug.Assert(InWorld);

            if(this.Geom != null)
            {
                Ode.dGeomDestroy(this.Geom);
            }

            if(this.Body != null)
            {
                Ode.dBodyDestroy(this.Body);
            }

            this.Geom = IntPtr.Zero;
            this.Body = IntPtr.Zero;
            InWorld = false;
        }

        public virtual void Serialize(BinaryWriter stream)
        {
            stream.Write((Int32)Id);
            stream.Write((Int16)ObjectClass);
            //TODO: write geom info
        }

        /// <summary>
        /// Updates the stream with the position. 
        /// </summary>
        /// <param name="stream"></param>
        public virtual void UpdateStream(BinaryWriter stream)
        {
            Ode.dVector3 m3 = Ode.dGeomGetPosition(this.Geom);
            stream.Write(m3.X);
            stream.Write(m3.Y);
            stream.Write(m3.Z);
        }
    }
}
