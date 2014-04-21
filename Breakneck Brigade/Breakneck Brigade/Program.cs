using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Tao.Glfw;
using Tao.OpenGl;
using System.IO;
using Breakneck_Brigade.Graphics;

namespace Breakneck_Brigade
{
    class Program
    {

        static void Main(string[] args)
        {
#if PROJECT_NETWORK_MODE
                (new FakeClient()).ShowDialog();
                return;
#endif

#if PROJECT_GRAPHICS_MODE
            Renderer renderer = new Renderer();
            while (Glfw.glfwGetWindowParam(Glfw.GLFW_OPENED) == Gl.GL_TRUE)
            {
                renderer.Render();
            }

            renderer.Destroy();
            Environment.Exit(0);
#endif

#if PROJECT_GAMECODE_TEST

#endif
        }
    }
}
