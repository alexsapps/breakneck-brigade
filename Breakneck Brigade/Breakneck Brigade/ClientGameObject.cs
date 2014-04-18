using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Breakneck_Brigade.Graphics;
using System.IO;

namespace Breakneck_Brigade
{
    abstract class ClientGameObject : IGameObject
    {
        protected ClientGame Game { get; set; }
        public int Id { get; set; }
        public Matrix4 Transform { get; set; }
        public Model Model { get; private set; }

        /// <summary>
        /// Base constructor. For every ClientGameObject, the parameters should be recieved from
        /// the Server, except for the client. The Id should corespond to a ServerGameObject id.  
        /// </summary>
        /// <param name="id">Should be unique across all client objects but the same as the 
        /// servers. Should be recieved from the server</param>
        /// <param name="transform">Initial position</param>
        /// <param name="game">The game where the object is created</param>
        public ClientGameObject(int id, Vector4 transform, ClientGame game) 
        {
            construct(id, new Matrix4(transform), game);
        }

        private void construct(int id, Matrix4 transform, ClientGame game) {
            //set properties that never change here
            this.Id = id;
            this.Game = game;
            update(transform);
        }

        private void update(Matrix4 transform)
        {
            //set properties that might change here.  this is seperated so it can be reused by public Update function.
            this.Transform = transform;
        }

        public static ClientGameObject Deserialize(int id, BinaryReader reader, ClientGame game)
        {
            GameObjectClass cls = (GameObjectClass)reader.ReadInt16();
            Type type = cls.GetGameObjectType();
            return (ClientGameObject)Activator.CreateInstance(type, id, reader, game);
        }

        public ClientGameObject(int id, BinaryReader reader, ClientGame game)
        {
            Matrix4 transform = new Matrix4(); //todo: read geom info from reader
            construct(id, transform, game);
        }

        /// <summary>
        /// Renders the game object in the world.
        /// </summary>
        public abstract void Render();

        public virtual void Update(BinaryReader reader)
        {
            var transform = new Matrix4(); //todo: read geom info from reader
            update(transform);
        }
    }
}
