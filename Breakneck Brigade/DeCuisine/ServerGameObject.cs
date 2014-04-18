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
        public GeomShape Shape { get; set; }

        public ServerGame Game;

        /// <summary>
        /// Base constructor. For every servergameobject create their should be a 
        /// coresponding ClientGameObject on each client with the same ID.
        /// </summary>
        /// <param name="id">Should be unique across all server objects but the same as clients</param>
        /// <param name="transform">Initial position</param>
        /// <param name="server">What server to put the object on</param>
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
            Game.ObjectRemoved(this);
        }

        public void AddToWorld()
        {
            Debug.Assert(!InWorld);
            

            if(HasBody)
                Body = Ode.dBodyCreate(Game.World);

            switch(Shape)
            {
                case GeomShape.Box:
                    //Geom = Ode.dCreateBox(space,  );
                    break;
                case GeomShape.Sphere:
                    //Geom = Ode.dCreateSphere(space, );
                    break;
                default:
                    throw new Exception("AddToWorld not defined for GeomShape of " + Shape.ToString());
            }

            Ode.dGeomSetData(Geom, new IntPtr(Id));

            InWorld = true;
        }

        public void RemoveFromWorld()
        {
            Debug.Assert(InWorld);
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
    }
}
