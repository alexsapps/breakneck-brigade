using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using Tao.Glfw;

namespace Breakneck_Brigade.Graphics
{
    /// <summary>
    /// A container class for textures for use with OpenGL 
    /// </summary>
    class Texture
    {
        const string TEXTURE_DIRECTORY = "res\\textures\\";

        public int TextureID;

        /// <summary>
        /// Loads a texture and stores it into the texture map by name
        /// </summary>
        /// <param name="filename">The filename of the texture in the ./res/textures directory</param>
        public Texture(string filename)
        {
            //Get a new texture ID and bind it
            int[] textureIDs = new int[1];
            Gl.glGenTextures(1,textureIDs);
            TextureID = textureIDs[0];
            this.Bind();

            //Load the texture
            string finalPath = TEXTURE_DIRECTORY + filename;
            int loadResult = Glfw.glfwLoadTexture2D(TEXTURE_DIRECTORY + filename, Glfw.GLFW_BUILD_MIPMAPS_BIT);
            if(loadResult == Gl.GL_FALSE)
            {
                Console.Error.WriteLine("Error loading file: " + filename);
            }
            
            //Billinear Filtering for decreases and increases in size
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_S, Gl.GL_REPEAT);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_WRAP_T, Gl.GL_REPEAT);
        }

        public void Bind()
        {
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, TextureID);
        }
    }
}
