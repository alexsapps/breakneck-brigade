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
        static Camera MainCamera;
        static List<Model> Models;
        private static int setDisplacement;

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

        static void testParser(string filename)
        {
            setDisplacement += 50;
            Parser parser = new Parser();
            Model model = parser.ParseFile(filename);
            model.Transformation.TranslationMat(setDisplacement, 0, 0);
            Models.Add(model);
        }

        /// <summary>
        /// Builds a snowman and adds it to the list of things to be rendered.
        /// </summary>
        static void makeAnnaHappy()
        {
            ColoredGluSphere snowmanBase    = new ColoredGluSphere(3, 10, 10);
            ColoredGluSphere snowmanBody    = new ColoredGluSphere(2.5, 10, 10,
                                                (new Matrix4()).TranslationMat(0.0f,4.0f,0.0f));
            ColoredGluSphere snowmanHead    = new ColoredGluSphere(2, 10, 10,
                                                (new Matrix4()).TranslationMat(0.0f,3.0f,0.0f));

            //Buttons
            ColoredGluSphere button0        = new ColoredGluSphere(0.5, 10, 10,
                                                (new Matrix4()).TranslationMat(0.0f, 0.0f, -2.25f),
                                                0, 0, 0, 1);
            ColoredGluSphere button1        = new ColoredGluSphere(0.5, 10, 10,
                                                (new Matrix4()).TranslationMat(0.0f, 1.0f, -2.0f),
                                                0, 0, 0, 1);
            ColoredGluSphere button2        = new ColoredGluSphere(0.5, 10, 10,
                                                (new Matrix4()).TranslationMat(0.0f, -1.0f, -2.0f),
                                                0, 0, 0, 1);

            //Face
            ColoredGluSphere eyeL           = new ColoredGluSphere(0.3, 10, 10,
                                                (new Matrix4()).TranslationMat(-0.75f, 1.0f, -1.75f),
                                                0, 0, 0, 1);
            ColoredGluSphere eyeR           = new ColoredGluSphere(0.3, 10, 10,
                                                (new Matrix4()).TranslationMat(0.75f, 1.0f, -1.75f),
                                                0, 0, 0, 1);
            ColoredGluSphere nose           = new ColoredGluSphere(0.3, 10, 10,
                                                (new Matrix4()).TranslationMat(0.0f, 0.5f, -2.0f),
                                                0, 0, 0, 1);
            ColoredGluSphere mouth0         = new ColoredGluSphere(0.3, 10, 10,
                                                (new Matrix4()).TranslationMat(0.95f, 0.2f, -1.75f),
                                                0, 0, 0, 1);
            ColoredGluSphere mouth1         = new ColoredGluSphere(0.3, 10, 10,
                                                (new Matrix4()).TranslationMat(0.35f, -0.1f, -2.0f),
                                                0, 0, 0, 1);
            ColoredGluSphere mouth2         = new ColoredGluSphere(0.3, 10, 10,
                                                (new Matrix4()).TranslationMat(-0.35f, -0.1f, -2.0f),
                                                0, 0, 0, 1);
            ColoredGluSphere mouth3         = new ColoredGluSphere(0.3, 10, 10,
                                                (new Matrix4()).TranslationMat(-0.95f, 0.2f, -1.75f),
                                                0, 0, 0, 1);


            snowmanHead.Children.Add(eyeL);
            snowmanHead.Children.Add(eyeR);
            snowmanHead.Children.Add(nose);
            snowmanHead.Children.Add(mouth0);
            snowmanHead.Children.Add(mouth1);
            snowmanHead.Children.Add(mouth2);
            snowmanHead.Children.Add(mouth3);

            snowmanBody.Children.Add(snowmanHead);
            snowmanBody.Children.Add(button0);
            snowmanBody.Children.Add(button1);
            snowmanBody.Children.Add(button2);

            snowmanBase.Children.Add(snowmanBody);

            Model snowman = new Model();
            snowman.Meshes.Add(snowmanBase);
            Models.Add(snowman);

        }

        /// <summary>
        /// Initalizes all settings for OpenGL
        /// </summary>
        static void InitGL()
        {
            /* LIGHTING */
            Gl.glEnable(Gl.GL_LIGHTING);

            float[] position        = { 0, 1000, 1000};
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
            //Enables manual setting of colors and materials of primatives (through glColor__, etc)
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            //Generates normals for objects which do not specify normals
            Gl.glEnable(Gl.GL_AUTO_NORMAL);
            //For basic polygons, draws both a front and back face. (May disable for performance reasons later)
            Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL); 
            //What shading model to use for rendering Gl prims
            Gl.glShadeModel(Gl.GL_SMOOTH);
            //Turn on texturing
            Gl.glEnable(Gl.GL_TEXTURE_2D);


            /* CAMERA */
            MainCamera = new Camera();
            MainCamera.Distance = 100.0f;
            MainCamera.Incline  = 0.0f;
            MainCamera.Transform.TranslationMat(0, -25, 0);

            Models = new List<Model>();

            /* TESTING MODES */
            //makeAnnaHappy(); //Do you want to build a snowman?
            testParser("orange.obj"); //Load a object file from the current dir
            testParser("orange.obj");
        }

        static void Render()
        {
            int width, height;
            Glfw.glfwGetWindowSize(out width, out height);
            float ratio = (float)width / height;
            //Re-init mouse stuff! Call InputMan's MousePosInit() method

            Gl.glViewport(0, 0, width, height);
            //Always clear both color and depth
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            MainCamera.Update();
            MainCamera.Render();

            foreach(Model model in Models)
            {
                //model.Transformation = model.Transformation * (new Matrix4()).RotateYDeg(5);
                model.Render();
            }

            Glfw.glfwSwapBuffers();
            // glfwSwapBuffers should implicitly call glfwPollEvents() by default
            // Glfw.glfwPollEvents();
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
