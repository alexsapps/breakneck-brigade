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

        public float xPos;
        public float yPos;
        public float zPos;

        public Vector4 LookingAt;
        public Vector4 Position;
        public Vector4 Up;

		public Camera() 
		{
            //Transform.TranslationMat(0, -25, 0);

            Transform = new Matrix4();
			Reset();

            xPos = 0.0f;
            yPos = 0.0f;
            zPos = 0.0f;

            LookingAt = new Vector4();
            Up = new Vector4();
		}

		public void Update(LocalPlayer cp) 
        {
            if (cp != null) //if not connected, or no current game, then no local palyer
            {
                Azimuth = cp.Orientation;
                Incline = cp.Incline;
                Up[1] = (float)Math.Cos(Incline * Math.PI / 180.0f);

                var pos = cp.GetPosition();
                xPos = pos[0];
                yPos = pos[1];
                zPos = pos[2];

                Position = new Vector4(xPos, yPos, zPos);

                anglesToAxis();
            }
        }

        public void Reset() 
        {
			FOV=75.0f;
			Aspect=1.33f;
			NearClip=0.1f;
			FarClip=1000.0f;

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
            Gl.glTranslatef(xPos, yPos, zPos);           

            Gl.glMultMatrixf(Transform.glArray);

			// Return to modelview matrix mode
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
		}

        public void anglesToAxis()
        {
            const float DEG2RAD = (float)Math.PI / 180.0f;
            float sx, sy, sz, cx, cy, cz, theta;

            //Vector4 side = new Vector4();
            //Vector4 up = new Vector4();
            Vector4 forward = new Vector4();

            // X-axis rotation
            theta = Incline * DEG2RAD;
            sx = (float)Math.Sin(theta);
            cx = (float)Math.Cos(theta);

            // Y-axis rotation
            theta = Azimuth * DEG2RAD;
            sy = (float)Math.Sin(theta);
            cy = (float)Math.Cos(theta);

            // Z-axis rotation
            theta = 0.0f * DEG2RAD;
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
            forward[0] = sy * cz + cy * sx * sz;
            forward[1] = sy * sz - cy * sx * cz;
            forward[2] = cy * cx;

            LookingAt.Set(xPos + forward[0], yPos + forward[1], zPos + forward[2], LookingAt.W);
        }
    }
}
