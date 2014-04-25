using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Tao.OpenGl;

namespace Breakneck_Brigade.Graphics
{
    /*
     * A container class for data associated with a model
     * Ryan George
     */
    class Model
    {
        private     Vector4             _position;
        public      Vector4             Position        { get { return this._position; }    set {this._position = value;  updateMatrix();} }
        private     Vector4             _scale;
        public      Vector4             Scale           { get { return this._scale; }       set {this._scale = value;     updateMatrix();} }
        private     Vector4             _rotation;
        public      Vector4             Rotation        { get { return this._rotation; }    set {this._rotation = value;  updateMatrix();} }
        
        private     Matrix4             Transformation  { get; set; }
        public      List<AObject3D>     Meshes          { get; private set; }

        public Model()
        {
            Meshes          = new List<AObject3D>();
            Transformation  = new Matrix4();
            _position       = new Vector4();
            _scale          = new Vector4();
            _rotation       = new Vector4();
        }

        public void Render()
        {
            updateMatrix();
            Gl.glPushMatrix();
            Gl.glLoadMatrixf(Transformation.glArray);
            foreach(AObject3D m in Meshes)
            {
                m.Render();
            }
            Gl.glPopMatrix();
        }

        /// <summary>
        /// Updates the matrix to reflect the properties of the model
        /// </summary>
        private void updateMatrix()
        {
            //Translate to location
            Transformation.TranslationMat(Position.X, Position.Y, Position.Z);
            
            //Rotate to proper orientation: 
            Transformation = Transformation*Matrix4.MakeRotateZ(Rotation.Z);
            Transformation = Transformation*Matrix4.MakeRotateY(Rotation.Y);
            Transformation = Transformation*Matrix4.MakeRotateX(Rotation.X);

            //Scale
            Transformation = Transformation*Matrix4.MakeScalingMat(Scale.X, Scale.Y, Scale.Z);
        }
    }
}
