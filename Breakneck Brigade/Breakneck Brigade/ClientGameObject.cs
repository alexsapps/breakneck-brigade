using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Breakneck_Brigade.Graphics;
using System.IO;
using Tao.OpenGl;

namespace Breakneck_Brigade
{
    abstract class ClientGameObject : IGameObject
    {
        private Vector4 _position;
        public Vector4 Position { get { return this._position; } set { this._position = value; updateMatrix(); } }
        private Vector4 _scale;
        public Vector4 Scale { get { return this._scale; } set { this._scale = value; updateMatrix(); } }
        private Vector4 _rotation;
        public Vector4 Rotation { get { return this._rotation; } set { this._rotation = value; updateMatrix(); } }
        public bool ToRender { get; set; }

        public Matrix4 Transformation { get; set; }

        protected ClientGame Game { get; set; }
        public int Id { get; set; }
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
            constructEssential(id, game);
            this.ToRender = true;
        }

        public ClientGameObject(int id, ClientGame game, Vector4 position)
            : this (id, game)
        {
            _position = position;
        }

        /// <summary>
        /// Instatiates client objects from data in the reader.
        /// The packet will look as follows. Class specific data will be written after this.
        /// 
        /// bool  toRender
        /// float x
        /// float y
        /// float z
        /// </summary>
        /// <param name="id"></param>
        /// <param name="game"></param>
        /// <param name="reader"></param>
        public ClientGameObject(int id, ClientGame game, BinaryReader reader)
        {
            constructEssential(id, game);
            
            this.ToRender = reader.ReadBoolean();
            readGeom(reader);
        }

        protected void constructEssential(int id, ClientGame game)
        {
            this.Id = id;
            this.Game = game;
        }

        //this function expects you to call updateMatrix later on, directly or through initGeom
        protected virtual void readGeom(BinaryReader reader)
        {
            _position = reader.ReadCoordinate();
        }

        protected void finalizeConstruction()
        {
            initGeom();
            Model = Renderer.Models[Renderer.Models.ContainsKey(ModelName) ? ModelName : "bread"];
            Scale = Model.InitialScale;
        }

        protected virtual void initGeom()
        {
            Transformation = new Matrix4();
            _position = _position ?? new Vector4();
            _scale = _scale ?? new Vector4(1.0f, 1.0f, 1.0f);
            _rotation = _rotation ?? new Vector4();
            updateMatrix();
        }

        /// <summary>
        /// Read the serialized data from the packet. 
        /// bool  toRender
        /// int16 objectclass
        /// float X
        /// float Y
        /// float Z
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
        /// Update values from the stream "server", The position is the only thing
        /// handled by the base class. 
        /// The packet looks as follows. Client specific data will be read after 
        /// this data.
        /// 
        /// bool  ToRender
        /// float x
        /// float y
        /// float z
        /// </summary>
        /// <param name="reader"></param>
        public virtual void StreamUpdate(BinaryReader reader)
        {
            this.ToRender = reader.ReadBoolean();
            
            readGeom(reader);
            updateMatrix();
        }

        /// <summary>
        /// Renders the game object in the world.
        /// </summary>
        public virtual void Render()
        {
            if (this.ToRender)
            {
                Gl.glPushMatrix();
                Gl.glMultMatrixf(Transformation.glArray);
                    Model.Render();
                Gl.glPopMatrix();
            }

        }

        /// <summary>
        /// Updates the position of the current objects transfrom.
        /// </summary>
        /// <param name="transform"></param>
        protected void BaseUpdate(Vector4 position)
        {
            this.Position = position;
        }

        protected virtual void updateMatrix()
        {
            Matrix4 finalMat = new Matrix4();
            //Translate to location
            finalMat.TranslationMat(Position.X, Position.Y, Position.Z);

            //Rotate to proper orientation: 
            finalMat = finalMat*Matrix4.MakeRotateZ(Rotation.Z);
            finalMat = finalMat*Matrix4.MakeRotateY(Rotation.Y);
            finalMat = finalMat*Matrix4.MakeRotateX(Rotation.X);
            
            //Scale
            finalMat = finalMat*Matrix4.MakeScalingMat(Scale.X, Scale.Y, Scale.Z);

            Transformation = finalMat;
        }

    }
}
