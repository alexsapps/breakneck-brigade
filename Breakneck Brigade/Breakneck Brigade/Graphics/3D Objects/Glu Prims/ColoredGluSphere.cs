using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Tao.OpenGl;

namespace Breakneck_Brigade.Graphics
{
    class ColoredGluSphere : AObject3D
    {
        /// <summary>
        /// A vector representing color:
        /// - Color[0] = Red
        /// - Color[1] = Green
        /// - Color[2] = Blue
        /// - Color[3] = Alpha
        /// </summary>
        public  Vector4     Color;
        /// <summary>
        /// The radius of the sphere
        /// </summary>
        public  double      Radius;
        /// <summary>
        /// The number of slices to use when rendering this sphere (i.e.: how many verticle cross sections)
        /// </summary>
        public  int         Slices;
        /// <summary>
        /// The number of stacks to use when rendering this sphere (i.e.: how many horizontal cross sections)
        /// </summary>
        public  int         Stacks;

        /// <summary>
        /// Draws a white GluSphere with no transformations (i.e.: at the origin)
        /// </summary>
        /// <param name="radius">Radius of the sphere</param>
        /// <param name="slices">The number of slices to use when rendering this sphere (i.e.: how many verticle cross sections)</param>
        /// <param name="stacks">The number of stacks to use when rendering this sphere (i.e.: how many horizontal cross sections)</param>
        public ColoredGluSphere(double radius, int slices, int stacks) : base()
        {
            Radius          = radius;
            Slices          = slices;
            Stacks          = stacks;
            Color           = new Vector4(1.0, 1.0, 1.0, 1.0);
        }

        /// <summary>
        /// Draws a transformed GluSphere
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="radius">Radius of the sphere</param>
        /// <param name="slices">The number of slices to use when rendering this sphere (i.e.: how many verticle cross sections)</param>
        /// <param name="stacks">The number of stacks to use when rendering this sphere (i.e.: how many horizontal cross sections)</param>
        public ColoredGluSphere(double radius, int slices, int stacks, Matrix4 trans) : base(trans)
        {
            Radius          = radius;
            Slices          = slices;
            Stacks          = stacks;
            Color           = new Vector4(1.0, 1.0, 1.0, 1.0);
        }

        /// <summary>
        /// Draws a GluSphere at the origin with the color specified
        /// </summary>
        /// <param name="radius">Radius of the sphere</param>
        /// <param name="slices">The number of slices to use when rendering this sphere (i.e.: how many verticle cross sections)</param>
        /// <param name="stacks">The number of stacks to use when rendering this sphere (i.e.: how many horizontal cross sections)</param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        public ColoredGluSphere(double radius, int slices, int stacks, float red, float green, float blue, float alpha) : base()
        {
            Radius          = radius;
            Slices          = slices;
            Stacks          = stacks;
            Color           = new Vector4(red, green, blue, alpha);
        }
        
        /// <summary>
        /// Draws a transformed GluSphere with the color specified
        /// </summary>
        /// <param name="radius">Radius of the sphere</param>
        /// <param name="slices">The number of slices to use when rendering this sphere (i.e.: how many verticle cross sections)</param>
        /// <param name="stacks">The number of stacks to use when rendering this sphere (i.e.: how many horizontal cross sections)</param>
        /// <param name="trans"></param>
        /// <param name="red"></param>
        /// <param name="green"></param>
        /// <param name="blue"></param>
        /// <param name="alpha"></param>
        public ColoredGluSphere(double radius, int slices, int stacks, Matrix4 trans, float red, float green, float blue, float alpha) : base(trans)
        {
            Radius          = radius;
            Slices          = slices;
            Stacks          = stacks;
            Color           = new Vector4(red, green, blue, alpha);
        }

        public override void Render()
        {
            Gl.glPushMatrix();
            Gl.glMultMatrixf(Transformation.glArray);
                //Store previous color
                float[] prevColor = new float[4]; 
                Gl.glGetFloatv(Gl.GL_CURRENT_COLOR, prevColor);
                Gl.glColor4f(Color[0], Color[1], Color[2], Color[3]);

                Glu.gluSphere(Renderer.gluQuadric,Radius,Slices, Stacks);

                //Restore previous color
                Gl.glColor4fv(prevColor);

                foreach(AObject3D child in Children)
                {
                    child.Render();
                }
            Gl.glPopMatrix();
        }
    }
}
