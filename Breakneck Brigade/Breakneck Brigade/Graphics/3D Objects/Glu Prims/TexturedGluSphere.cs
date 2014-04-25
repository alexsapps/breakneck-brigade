using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Tao.OpenGl;

namespace Breakneck_Brigade.Graphics
{
    class TexturedGluSphere : AObject3D
    {
        /// <summary>
        /// The texture to use on this sphere
        /// </summary>
        public  Texture     Texture;
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
        public TexturedGluSphere(double radius, int slices, int stacks) : base()
        {
            Radius          = radius;
            Slices          = slices;
            Stacks          = stacks;
            Texture         = Renderer.DefaultTexture;
        }

        /// <summary>
        /// Draws a transformed GluSphere
        /// </summary>
        /// <param name="trans"></param>
        /// <param name="radius">Radius of the sphere</param>
        /// <param name="slices">The number of slices to use when rendering this sphere (i.e.: how many verticle cross sections)</param>
        /// <param name="stacks">The number of stacks to use when rendering this sphere (i.e.: how many horizontal cross sections)</param>
        public TexturedGluSphere(double radius, int slices, int stacks, Matrix4 trans) : base(trans)
        {
            Radius          = radius;
            Slices          = slices;
            Stacks          = stacks;
            Texture         = Renderer.DefaultTexture;
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
        public TexturedGluSphere(double radius, int slices, int stacks, Texture texture) : base()
        {
            Radius          = radius;
            Slices          = slices;
            Stacks          = stacks;
            Texture         = texture;
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
        public TexturedGluSphere(double radius, int slices, int stacks, Matrix4 trans, Texture texture) : base(trans)
        {
            Radius          = radius;
            Slices          = slices;
            Stacks          = stacks;
            Texture         = texture;
        }

        public override void Render()
        {
            Gl.glPushMatrix();
            Gl.glMultMatrixf(_transform.glArray);
                //Store previous color
                float[] prevColor = new float[4]; 
                Gl.glGetFloatv(Gl.GL_CURRENT_COLOR, prevColor);
                Gl.glColor4f(1.0f, 1.0f, 1.0f, 1.0f);

                Glu.gluQuadricTexture(Renderer.gluQuadric, Gl.GL_TRUE);      //Enables texturing of Glu opbjects
                Texture.Bind();
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
