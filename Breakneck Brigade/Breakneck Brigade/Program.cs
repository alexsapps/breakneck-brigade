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
        /**
         * Runs GLFW initialization code, terminiating if initialization failed
         */
        static Camera mainCamera;

        /// <summary>
        /// Initializes all settings for GLFW (window rendering and handling)
        /// </summary>
        static void InitGLFW()
        {
            if(Glfw.glfwInit() == Gl.GL_FALSE)
            {
                Console.Error.WriteLine("ERROR: GLFW Initialization failed!");
                Environment.Exit(1);
            }
            Glfw.glfwOpenWindow(640, 480, 0, 0, 0, 8, 32, 32, Glfw.GLFW_WINDOW);
        }

        static void DestroyGLFW()
        {
            Glfw.glfwCloseWindow();
            Glfw.glfwTerminate();
        }

        /// <summary>
        /// Initalizes all settings for OpenGL
        /// </summary>
        static void InitGL()
        {
            /* LIGHTING */
            Gl.glEnable(Gl.GL_LIGHTING);
            float[] position        = { 0, 5, -1 };
            float[] ambientColor    = { 0, 0, 0, 1 };
            float[] diffuseColor    = { 1, 1, 1, 1 };
            float[] specularColor   = { 1, 1, 1, 1 };
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, position);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, ambientColor);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, diffuseColor);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPECULAR, specularColor);
            Gl.glEnable(Gl.GL_LIGHT0);

            /* RENDERING SETTINGS */
            //Enables depth buffering for standard GL calls (glu rendering calls, etc.)
            Gl.glEnable(Gl.GL_DEPTH_TEST); 
            //For basic polygons, draws both a front and back face. (May disable for performance reasons later)
            Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL); 
            //What shading model to use for rendering Gl prims
            Gl.glShadeModel(Gl.GL_SMOOTH);

            /* CAMERA */
            mainCamera = new Camera();
            mainCamera.Distance = 50.0f;
            mainCamera.Incline  = 20.0f;
        }

        static void DrawSphere(float x, float y, float z, float radius)
        {
            Matrix4 transform = new Matrix4();
            transform = transform * ((new Matrix4()).TranslationMat(x, y, z));
            Gl.glPushMatrix();
            Gl.glLoadMatrixf(transform.glArray);
            Glu.GLUquadric quad = Glu.gluNewQuadric();
            Glu.gluSphere(quad, radius, 20, 20);
            Gl.glPopMatrix();
        }

        static void Render()
        {
            int width, height;
            Glfw.glfwGetWindowSize(out width, out height);
            float ratio = (float)width / height;

            Gl.glViewport(0, 0, width, height);
            //Always clear both color and depth
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            mainCamera.Update();
            mainCamera.Render();

            DrawSphere(1, 1, 1, 2);
            DrawSphere(-20, 1, 5, 2);
            DrawSphere(0, 0, 10, 2);

            Glfw.glfwSwapBuffers();
            Glfw.glfwPollEvents();
        }

        static void Main(string[] args)
        {
#if PROJECT_NETWORK_MODE
                (new FakeClient()).ShowDialog();
                return;
#endif

#if PROJECT_GRAPHICS_MODE
            InitGLFW();
            InitGL();

            while (Glfw.glfwGetWindowParam(Glfw.GLFW_OPENED) == Gl.GL_TRUE)
            {
                Render();
            }

            DestroyGLFW();
            Environment.Exit(0);
#endif

#if PROJECT_GAMECODE_TEST

#endif
        }
    }
}
