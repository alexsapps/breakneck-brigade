using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

namespace DeCuisine
{
    /// <summary>
    /// Top level server game object. All server game objects should inherit. 
    /// </summary>
    public abstract class ServerGameObject : IGameObject
    {
        public int Id { get; set; }
        public Matrix4 Transform { get; set; }
        Server Server;

        /// <summary>
        /// Base constructor. For every servergameobject create their should be a 
        /// coresponding ClientGameObject on each client with the same ID.
        /// </summary>
        /// <param name="id">Should be unique across all server objects but the same as clients</param>
        /// <param name="transform">Initial position</param>
        /// <param name="server">What server to put the object on</param>
        public ServerGameObject(int id, Vector4 transform, Server server ) 
        {
            this.Id = id;
            this.Server = server;
            this.Transform = new Matrix4(transform);
        }

        /// <summary>
        /// Specific to each subclass. 
        /// </summary>
        abstract void Update();
    }
}
