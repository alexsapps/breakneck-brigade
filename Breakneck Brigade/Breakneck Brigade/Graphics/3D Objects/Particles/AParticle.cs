using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

namespace Breakneck_Brigade.Graphics
{
    /// <summary>
    /// An abstract class representing a particle
    /// </summary>
    abstract class AParticle
    {
        /// <summary>
        /// Current position of the particle
        /// </summary>
        public      Vector4 Position;
        /// <summary>
        /// Current velocity of the particle
        /// </summary>
        public      Vector4 Velocity;
        /// <summary>
        /// Current acceleration of the particle
        /// </summary>
        public      Vector4 Acceleration;
        /// <summary>
        /// The current rotation of the particle in deg
        /// </summary>
        public Vector4 Rotation; 
        /// <summary>
        /// Rotational velocity of the particle (X, Y, Z) in deg/tick
        /// </summary>
        public      Vector4 RotationalVelocity;

        public      float   Scale;

        public      float   Lifetime;
        protected   float   _age { get; set; }
        public      bool    ShouldDie;


        protected   Matrix4 _transform;

        public AParticle()
        {
            Position            = new Vector4(0,0,0);
            Velocity            = new Vector4(0,0,0);
            Acceleration        = new Vector4(0,0,0);
            Scale               = 1.0f;
            RotationalVelocity  = new Vector4(0, 0, 0);
            Rotation            = new Vector4(0, 0, 0);
            Lifetime            = 10.0f;
            _age                 = 0.0f;
            ShouldDie           = false;
            _transform          = new Matrix4();
        }

        public AParticle(Vector4 position, Vector4 velocity, Vector4 acceleration, float scale, float lifetime)
        {
            Position            = position;
            Velocity            = velocity;
            Acceleration        = acceleration;
            RotationalVelocity  = new Vector4(0, 0, 0);
            Rotation            = new Vector4(0, 0, 0);
            Scale               = scale;
            Lifetime            = lifetime;
            _age                 = 0.0f;
            ShouldDie           = false;
            _transform          = new Matrix4();
        }

        /// <summary>
        /// Updates the particle. The base method updates the position,
        /// velocity, acceleration, and transformation matrix as well as the age.
        /// </summary>
        /// <param name="timestep">The amount of time that has passed since the particle was last updated</param>
        public virtual void Update(float timestep)
        {
            if(_age == 0.0f)
            {
                //OnCreate();
            }
            _age += timestep;
            if (Lifetime != -1)
            {
                if (_age > Lifetime)
                {
                    ShouldDie = true;
                    //OnDestroy();
                    return; // Don't need to continue to updating
                }
            }
            Velocity += Acceleration*timestep;
            Position += Velocity*timestep;

            Rotation += RotationalVelocity;

            updateMatrix();
        }
        
        protected void updateMatrix()
        {
            _transform.Identity();
            _transform = _transform * Matrix4.MakeTranslationMat(Position.X, Position.Y, Position.Z);

            _transform = _transform * Matrix4.MakeRotateXDeg(Rotation.X);
            _transform = _transform * Matrix4.MakeRotateYDeg(Rotation.Y);
            _transform = _transform * Matrix4.MakeRotateZDeg(Rotation.Z);

            _transform = _transform * Matrix4.MakeScalingMat(Scale, Scale, Scale);
        }

        /// <summary>
        /// Function called to render the particle
        /// </summary>
        public abstract void Render();
        /// <summary>
        /// Function called when the particle is created
        /// </summary>
        public virtual void OnCreate()
        {
        }
        /// <summary>
        /// Function called when the particle is destroyed
        /// </summary>
        public virtual void OnDestroy()
        { 
        }
    }
}
