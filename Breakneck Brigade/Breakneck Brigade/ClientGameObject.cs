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
        private Matrix4 _rotation;
        public Matrix4 Rotation { get { return this._rotation; } set { this._rotation = value; updateMatrix(); } }
        public bool ToRender { get; set; }
        public Vector4 minBound, maxBound;

        public Matrix4 Transformation { get; set; }

        protected ClientGame Game { get; set; }
        public int Id { get; set; }
        public abstract float[] Sides { get; }
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
            _rotation = reader.ReadRotation();
            minBound = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            maxBound = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
        }

        protected void finalizeConstruction()
        {
            initGeom();
            Model = Renderer.Models[Renderer.Models.ContainsKey(ModelName) ? ModelName : "bread"];
            //Model = Renderer.Models["sugar"];
        }

        protected virtual void initGeom()
        {
            Transformation = new Matrix4();
            _position = _position ?? new Vector4();
            _scale = _scale ?? new Vector4(1.0f, 1.0f, 1.0f);
            _rotation = _rotation ?? new Matrix4();
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
                
                //Wire collision box
                
                Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_LINE);
                Gl.glDisable(Gl.GL_TEXTURE);
                Gl.glDisable(Gl.GL_CULL_FACE);
                if(Sides.Length == 1) //Sphere
                {
                    Glu.gluSphere(Renderer.gluQuadric, (maxBound - minBound).Magnitude() / 2, 10, 10);
                }
                else if(Sides.Length == 3) //Cube
                {
                    Vector4 F0, F1, F2, F3, B0, B1, B2, B3;
                    F0 = maxBound;
                    F1 = new Vector4(minBound.X, maxBound.Y, maxBound.Z);
                    F2 = new Vector4(minBound.X, minBound.Y, maxBound.Z);
                    F3 = new Vector4(maxBound.X, minBound.Y, maxBound.Z);

                    B0 = minBound;
                    B1 = new Vector4(maxBound.X, minBound.Y, minBound.Z);
                    B2 = new Vector4(maxBound.X, maxBound.Y, minBound.Z);
                    B3 = new Vector4(minBound.X, maxBound.Y, minBound.Z);
                    Gl.glBegin(Gl.GL_QUADS);
                        //Front
                        Gl.glVertex3f(F0.X, F0.Y, F0.Z);
                        Gl.glVertex3f(F3.X, F3.Y, F3.Z);
                        Gl.glVertex3f(F2.X, F2.Y, F2.Z);
                        Gl.glVertex3f(F1.X, F1.Y, F1.Z);
                        //Back
                        Gl.glVertex3f(B0.X, B0.Y, B0.Z);
                        Gl.glVertex3f(B3.X, B3.Y, B3.Z);
                        Gl.glVertex3f(B2.X, B2.Y, B2.Z);
                        Gl.glVertex3f(B1.X, B1.Y, B1.Z);                      
                        //Left
                        Gl.glVertex3f(B0.X, B0.Y, B0.Z);
                        Gl.glVertex3f(B3.X, B3.Y, B3.Z);
                        Gl.glVertex3f(F1.X, F1.Y, F1.Z);
                        Gl.glVertex3f(F2.X, F2.Y, F2.Z);
                        //Right
                        Gl.glVertex3f(F0.X, F0.Y, F0.Z);
                        Gl.glVertex3f(B2.X, B2.Y, B2.Z);
                        Gl.glVertex3f(B1.X, B1.Y, B1.Z);
                        Gl.glVertex3f(F3.X, F3.Y, F3.Z);
                        //Don't need to render top or bottom - lines already drawn
                    Gl.glEnd();
                }
                Gl.glEnable(Gl.GL_TEXTURE);
                Gl.glPolygonMode(Gl.GL_FRONT, Gl.GL_FILL);
                Gl.glEnable(Gl.GL_CULL_FACE);
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
            finalMat = finalMat*Rotation;
            
            //Scale
            finalMat = finalMat*Matrix4.MakeScalingMat(Scale.X, Scale.Y, Scale.Z);

            Transformation = finalMat;
        }

    }
}
