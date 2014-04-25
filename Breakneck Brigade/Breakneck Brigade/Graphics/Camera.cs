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
    public class Camera
    {
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

		public Camera() 
		{
            Transform = new Matrix4();
			Reset();

            xPos = 0.0f;
            yPos = 0.0f;
            zPos = 0.0f;
		}

		public void Update(ClientPlayer cp) 
        {
            Azimuth = cp.Orientation;
            Incline = cp.Incline;

            xPos = cp.Position[0];
            yPos = cp.Position[1];
            zPos = cp.Position[2];
        }

        public void Reset() 
        {
			FOV=60.0f;
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
            Gl.glTranslatef(0, 0, -Distance);
			Gl.glRotatef(Incline,1.0f,0.0f,0.0f);
			Gl.glRotatef(Azimuth,0.0f,1.0f,0.0f);

            

            Gl.glMultMatrixf(Transform.glArray);

			// Return to modelview matrix mode
			Gl.glMatrixMode(Gl.GL_MODELVIEW);
		}

    }
}
