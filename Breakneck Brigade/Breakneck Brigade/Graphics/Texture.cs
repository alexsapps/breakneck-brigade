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
        public int TextureID;

        public Texture(string pathToFile)
        {
            //Get a new texture ID and bind it
            int[] textureIDs = new int[1];
            Gl.glGenTextures(1,textureIDs);
            TextureID = textureIDs[0];
            this.Bind();

            //Load the texture
            Glfw.glfwLoadTexture2D(pathToFile, Glfw.GLFW_BUILD_MIPMAPS_BIT);
            
            //Billinear Filtering for decreases and increases in size
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MIN_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
            Gl.glTexParameteri(Gl.GL_TEXTURE_2D, Gl.GL_TEXTURE_MAG_FILTER, Gl.GL_LINEAR_MIPMAP_LINEAR);
        }

        public void Bind()
        {
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, TextureID);
        }
    }
}
