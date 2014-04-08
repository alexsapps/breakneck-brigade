using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Tao.Glfw;
using Tao.OpenGl;
using System.IO;

namespace Breakneck_Brigade
{
    class Program
    {
        /**
         * Runs GLFW initialization code, terminiating if initialization failed
         */
        static void InitGLFW()
        {
            if(Glfw.glfwInit() == Gl.GL_FALSE)
            {
                Console.Error.WriteLine("ERROR: GLFW Initialization failed!");
                Environment.Exit(1);
            }
        }

        static void Main(string[] args)
        {
#if PROJECT_NETWORK_MODE
                (new FakeClient()).ShowDialog();
                return;
#endif

#if PROJECT_GRAPHICS_MODE
            InitGLFW();
            Glfw.glfwOpenWindow(640, 480, 0, 0, 0, 8, 32, 32, Glfw.GLFW_WINDOW);

            while (Glfw.glfwGetWindowParam(Glfw.GLFW_OPENED) == Gl.GL_TRUE)
            {
                int width, height;
                Glfw.glfwGetWindowSize(out width, out height);
                float ratio = (float)width / height;

                //GL.Enable(EnableCap.Lighting);

                Matrix4 matrix4 = new Matrix4();
                //GL.LoadMatrix(matrix4.m);
             
                Gl.glViewport(0, 0, width, height);
                Gl.glClear(Gl.GL_COLOR_BUFFER_BIT);

                Gl.glMatrixMode(Gl.GL_PROJECTION);
                Gl.glLoadIdentity();
                Gl.glOrtho(-ratio, ratio, -1.0, 1.0, 1.0, -1.0);

                Gl.glMatrixMode(Gl.GL_MODELVIEW);
                Gl.glLoadIdentity();
                Gl.glRotatef((float) (Glfw.glfwGetTime() * 50.0), 0.0f, 0.0f, 1.0f);

                Gl.glBegin(Gl.GL_TRIANGLES);
                    Gl.glColor3f(.1f, 0.0f, 0.0f);
                    Gl.glVertex3f(-.6f, -.4f, 0.0f);
                    Gl.glColor3f(0.0f, .1f, 0.0f);
                    Gl.glVertex3f(.6f, -.4f, 0.0f);
                    Gl.glColor3f(0.0f, 0.0f, .10f);
                    Gl.glVertex3f(0.0f, .6f, 0.0f);
                Gl.glEnd();

                Glfw.glfwSwapBuffers();
                Glfw.glfwPollEvents();
            }

            Glfw.glfwCloseWindow();
            Glfw.glfwTerminate();
            Environment.Exit(0);
#endif

#if PROJECT_GAMECODE_TEST

#endif
        }
    }
}
