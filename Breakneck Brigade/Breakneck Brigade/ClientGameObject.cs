﻿using System;
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
        public Vector4 Transform { get; set; }
        public Model Model { get; protected set; }
        public abstract string ModelName { get; }

        /// <summary>
        /// Base constructor. For every ClientGameObject, the parameters should be recieved from
        /// the Server, except for the client. The Id should corespond to a ServerGameObject id.  
        /// </summary>
        /// <param name="id">Should be unique across all client objects but the same as the 
        /// servers. Should be recieved from the server</param>
        /// <param name="transform">Initial position</param>
        /// <param name="game">The game where the object is created</param>
        public ClientGameObject(int id, ClientGame game) 
        {
            this.Id = id;
            this.Game = game;
        }


        /// <summary>
        /// Instatiates client objects from data in the reader.
        /// The packet will look as follows. Class specific data will be written after this. 
        /// double x
        /// double y
        /// double z
        /// </summary>
        /// <param name="id"></param>
        /// <param name="game"></param>
        /// <param name="reader"></param>
        public ClientGameObject(int id, ClientGame game, BinaryReader reader)
        {
            this.Id = id;
            this.Game = game;
            double x, y, z;
            x = reader.ReadDouble();
            y = reader.ReadDouble();
            z = reader.ReadDouble();
            this.Transform =  new Vector4(x, y, z);
        }


        /// <summary>
        /// Read the serialized data from the packet. 
        /// int16  objectclass
        /// double X
        /// double Y
        /// double Z
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static ClientGameObject Deserialize(int id, BinaryReader reader, ClientGame game)
        {
            GameObjectClass cls = (GameObjectClass)reader.ReadInt16();
            Type type = cls.GetGameObjectType();
            return (ClientGameObject)Activator.CreateInstance(type, id, reader, game);
        }

        /// <summary>
        /// Update values from the stream "server", just the posistions are handled
        /// in the base class. Should call the Update for the transform and 
        /// be overriden by the specific update implementations
        /// function as well
        /// </summary>
        /// <param name="reader"></param>
        public virtual void StreamUpdate(BinaryReader reader)
        {
            Vector4 NewPosition = getPositionMatrix(reader);
            Update(NewPosition);
        }

        /// <summary>
        /// Renders the game object in the world.
        /// </summary>
        public virtual void Render()
        {
            Model.Render();
        }

        /// <summary>
        /// the next three values to be read from the reader should be the x, y 
        /// and z coordinates.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private Vector4 getPositionMatrix(BinaryReader reader)
        {
            double x, y, z;
            x = reader.ReadDouble();
            y = reader.ReadDouble();
            z = reader.ReadDouble();
            return new Vector4(x, y, z);
        }

        /// <summary>
        /// Updates the position of the current objects transfrom.
        /// </summary>
        /// <param name="transform"></param>
        public void Update(Vector4 transform)
        {
            this.Transform = transform; 
        }

    }
}
