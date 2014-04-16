using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Breakneck_Brigade.Graphics;

namespace Breakneck_Brigade
{
    abstract class ClientGameObject : IGameObject
    {
        public int Id { get; set; }
        public Matrix4 Transform { get; set; }
        public Model Model;
        Client Client;

        /// <summary>
        /// Base constructor. For every ClientGameObject, the parameters should be recieved from
        /// the Server, except for the client. The Id should corespond to a ServerGameObject id.  
        /// </summary>
        /// <param name="id">Should be unique across all client objects but the same as the 
        /// servers. Should be recieved from the server</param>
        /// <param name="transform">Initial position</param>
        /// <param name="client">The client where the object is created</param>
        /// <param name="model">What model is used to represent the object in the game world</param>
        public ClientGameObject(int id, Vector4 transform, Client client, Model model) 
        {
            this.Id = id;
            this.Client = client;
            this.Transform = new Matrix4(transform);
            this.Model = model;
        }

        /// <summary>
        /// Renders the game object in the world.
        /// </summary>
        public abstract void Render();
    }
}
