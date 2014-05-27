using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using SousChef;

namespace Breakneck_Brigade.Graphics
{
	/// <summary>
	/// Simple camera class with support for basic camera controls.
	/// </summary>
    class Camera
    {
        public BBLock Lock = new BBLock();

        // Perspective controls
        /// <summary>
        /// Field of View Angle in degrees
        /// </summary>
        public float FOV;		
        /// <summary>
        /// Aspect Ratio
        /// </summary>
        public float Aspect;
        /// <summary>
        /// Near clipping plane distance
        /// </summary>
        public float NearClip;
        /// <summary>
        /// Far clipping plane distance
        /// </summary>
        public float FarClip;

        // Polar controls
        /// <summary>
        /// Distance of the camera eye position to the origin in degrees
        /// </summary>
        public float Distance;
        /// <summary>
        /// Rotation of the camera eye position around the Y axis in degrees
        /// </summary>
        public float Azimuth;
        /// <summary>
        /// Angle of the camera eye position over the XZ plane in degrees
        /// </summary>
        public float Incline;

        public Matrix4 Transform;

        public Vector4 LookingAt;
        public Vector4 Position;
        public Vector4 Up;

		public Camera() 
		{
            //Transform.TranslationMat(0, -25, 0);

            Transform = new Matrix4();
			Reset();

            Position = new Vector4();

            LookingAt = new Vector4();
            Up = new Vector4();
		}

		public void Update(LocalPlayer cp) 
        {
            if (cp != null) //if not connected, or no current game, then no local palyer
            {
                Azimuth = cp.Orientation;
                Incline = cp.Incline;
                Up.Y = (float)Math.Cos(Incline * Math.PI / 180.0f);

                Position = cp.GetPosition();
                Position.Y += 8; //eyes are a little above the center of mass

                anglesToAxis();
            }
        }

        public void Reset() 
        {
			FOV=75.0f;
			Aspect=1.33f;
			NearClip=0.1f;
			FarClip=5000.0f;

			Distance=50.0f;
			Azimuth=0.0f;
			Incline=0.0f;
            Transform.Identity();
		}

        public void Render() 
        {
			// Tell GL we are going to adjust the projection matrix
			Gl.glMatrixMode(Gl.GL_PROJECTION);
			Gl.glLoadIdentity();

			// Set perspective projection
			Glu.gluPerspective(FOV,Aspect,NearClip,FarClip);

			// Place camera            
			Gl.glRotatef(Incline,1.0f,0.0f,0.0f);
			Gl.glRotatef(Azimuth,0.0f,1.0f,0.0f);
            Gl.glTranslatef(-(Position.X), -(Position.Y), -(Position.Z));           

            Gl.glMultMatrixf(Transform.glArray);

			// Return to modelview matrix mode
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
		}

        public void anglesToAxis()
        {
            float sx, sy, sz, cx, cy, cz, theta;

            //Vector4 side = new Vector4();
            //Vector4 up = new Vector4();

            // X-axis rotation
            theta = Incline * MathConstants.DEG2RAD;
            sx = (float)Math.Sin(theta);
            cx = (float)Math.Cos(theta);

            // Y-axis rotation
            theta = Azimuth * MathConstants.DEG2RAD;
            sy = (float)Math.Sin(theta);
            cy = (float)Math.Cos(theta);

            // Z-axis rotation
            theta = 0.0f * MathConstants.DEG2RAD;
            sz = (float)Math.Sin(theta);
            cz = (float)Math.Cos(theta);

            /* Unused
            // determine side vector
            side[0] = cy * cz - sy * sx * sz;
            side[1] = cy * sz + sy * sx * cz;
            side[2] = -sy * cx;

            // determine up vector
            up[0] = -cx * sz;
            up[1] = cx * cz;
            up[2] = sx;
             */

            // determine forward vector
            var forward = new Vector4()
            {
                X = sy * cz + cy * sx * sz,
                Y = sy * sz - cy * sx * cz,
                Z = cy * cx
            };

            var lookingAt = Position + forward;
            LookingAt.Set(lookingAt.X, lookingAt.Y, lookingAt.Z, LookingAt.W);
        }
    }
}
