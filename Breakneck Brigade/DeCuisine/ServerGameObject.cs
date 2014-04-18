using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using System.IO;

namespace DeCuisine
{
    /// <summary>
    /// Top level server game object. All server game objects should inherit. 
    /// </summary>
    abstract class ServerGameObject : IGameObject
    {
        public int Id { get; set; }
        public abstract GameObjectClass ObjectClass { get; }
        public Matrix4 Transform { get; set; }
        public ServerGame Game;

        /// <summary>
        /// Base constructor. For every servergameobject create their should be a 
        /// coresponding ClientGameObject on each client with the same ID.
        /// </summary>
        /// <param name="id">Should be unique across all server objects but the same as clients</param>
        /// <param name="transform">Initial position</param>
        /// <param name="server">What server to put the object on</param>
        public ServerGameObject(int id, Vector4 transform, ServerGame game) 
        {
            this.Id = id;
            this.Game = game;
            this.Transform = new Matrix4(transform);

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

        public virtual void Serialize(BinaryWriter stream)
        {
            stream.Write((Int32)Id);
            stream.Write((Int16)ObjectClass);
            //TODO: write geom info
        }
    }
}
