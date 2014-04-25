using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;
using Tao.Glfw;
using SousChef;

namespace Breakneck_Brigade.Graphics
{
    class Renderer : IDisposable
    {
        /// <summary>
        /// A mapping of filename to Model
        /// </summary>
        public static Dictionary<string, Model>     Model       = new Dictionary<string, Model>();
        /// <summary>
        /// A mapping of filename to Texture
        /// </summary>
        public static Dictionary<string, Texture>   Textures    = new Dictionary<string, Texture>();

        /// <summary>
        /// The default checkerboard texture
        /// </summary>
        public static Texture   DefaultTexture;

        public Matrix4                     WorldTransform;
        public List<ClientGameObject>      GameObjects;
        public Camera                      Camera;

        /// <summary>
        /// A singleton gluQuadric for use in Glu primative rendering functions
        /// </summary>
        public static Glu.GLUquadric gluQuadric = Glu.gluNewQuadric();
        /// <summary>
        /// A singleton gluTesselator for Glu tesselation
        /// </summary>
        public static Glu.GLUtesselator gluTesselator = Glu.gluNewTess();

        public Renderer()
        {
            WorldTransform      = new Matrix4();
            GameObjects         = new List<ClientGameObject>();

            InitGLFW();
            InitGL();
            InitGlu();

            DefaultTexture = new Texture("default.tga");

            /* TESTING MODES */
            //makeAnnaHappy(); //Do you want to build a snowman?
            testParser("orange"); //Load a object file from the current dir
            //testParser("orange.obj");
        }

        public void Render(ClientPlayer cp)
        {
            int width, height;
            Glfw.glfwGetWindowSize(out width, out height);
            float ratio = (float)width / height;
            //Re-init mouse stuff! Call InputMan's MousePosInit() method

            Gl.glViewport(0, 0, width, height);
            //Always clear both color and depth
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);

            Camera.Update(cp);
            Camera.Render();

            foreach(ClientGameObject cgo in GameObjects)
            {     
                cgo.Render();
            }

            Glfw.glfwSwapBuffers();
            // glfwSwapBuffers should implicitly call glfwPollEvents() by default
            // Glfw.glfwPollEvents();
        }

        /// <summary>
        /// Runs all cleanup functions on objects needed by the renderer
        /// </summary>
        public void Dispose()
        {
            DestroyGLFW();
        }

         /// <summary>
        /// Initalizes all settings for OpenGL
        /// </summary>
        private void InitGL()
        {
            /* LIGHTING */
            Gl.glEnable(Gl.GL_LIGHTING);

            float[] position        = { 0, 1000, 1000 };
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
            Camera = new Camera();
            Camera.Distance = 50.0f;
            Camera.Incline  = 0.0f;
            //MainCamera.Transform.TranslationMat(0, -25, 0);
        }

        /// <summary>
        /// Initializes some settings for Glu
        /// </summary>
        public void InitGlu()
        {
            Glu.gluQuadricNormals(gluQuadric, Glu.GLU_SMOOTH);  //Enables smooth normals on Glu rendered objects
            Glu.gluQuadricTexture(gluQuadric, Gl.GL_TRUE);      //Enables texturing of Glu opbjects
        }

        /// <summary>
        /// Initializes all settings for GLFW (window rendering and handling)
        /// </summary>
        private void InitGLFW()
        {
            if (Glfw.glfwInit() == Gl.GL_FALSE)
            {
                Console.Error.WriteLine("ERROR: GLFW Initialization failed!");
                Environment.Exit(1);
            }
            Glfw.glfwOpenWindow(1280, 1024, 0, 0, 0, 8, 32, 32, Glfw.GLFW_WINDOW);
        }

        /// <summary>
        /// Runs all cleanup functions for GLFW
        /// </summary>
        private void DestroyGLFW()
        {
            Glfw.glfwCloseWindow();
            Glfw.glfwTerminate();
        }

        /// <summary>
        /// Builds a snowman and adds it to the list of things to be rendered.
        /// </summary>
        private void makeAnnaHappy()
        {
            TexturedGluSphere snowmanBase    = new TexturedGluSphere(3, 10, 10);
            TexturedGluSphere snowmanBody    = new TexturedGluSphere(2.5, 10, 10,
                                                (new Matrix4()).TranslationMat(0.0f, 4.0f, 0.0f));
            TexturedGluSphere snowmanHead    = new TexturedGluSphere(2, 10, 10,
                                                (new Matrix4()).TranslationMat(0.0f, 3.0f, 0.0f));

            //Buttons
            TexturedGluSphere button0        = new TexturedGluSphere(0.5, 10, 10,
                                                (new Matrix4()).TranslationMat(0.0f, 0.0f, -2.25f));
            TexturedGluSphere button1        = new TexturedGluSphere(0.5, 10, 10,
                                                (new Matrix4()).TranslationMat(0.0f, 1.0f, -2.0f));
            TexturedGluSphere button2        = new TexturedGluSphere(0.5, 10, 10,
                                                (new Matrix4()).TranslationMat(0.0f, -1.0f, -2.0f));

            //Face
            TexturedGluSphere eyeL           = new TexturedGluSphere(0.3, 10, 10,
                                                (new Matrix4()).TranslationMat(-0.75f, 1.0f, -1.75f));
            TexturedGluSphere eyeR           = new TexturedGluSphere(0.3, 10, 10,
                                                (new Matrix4()).TranslationMat(0.75f, 1.0f, -1.75f));
            TexturedGluSphere nose           = new TexturedGluSphere(0.3, 10, 10,
                                                (new Matrix4()).TranslationMat(0.0f, 0.5f, -2.0f));
            TexturedGluSphere mouth0         = new TexturedGluSphere(0.3, 10, 10,
                                                (new Matrix4()).TranslationMat(0.95f, 0.2f, -1.75f));
            TexturedGluSphere mouth1         = new TexturedGluSphere(0.3, 10, 10,
                                                (new Matrix4()).TranslationMat(0.35f, -0.1f, -2.0f));
            TexturedGluSphere mouth2         = new TexturedGluSphere(0.3, 10, 10,
                                                (new Matrix4()).TranslationMat(-0.35f, -0.1f, -2.0f));
            TexturedGluSphere mouth3         = new TexturedGluSphere(0.3, 10, 10,
                                                (new Matrix4()).TranslationMat(-0.95f, 0.2f, -1.75f));


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

            TestClientGameObject snowmanGO = new TestClientGameObject(snowman);
            GameObjects.Add(snowmanGO);
        }

        void testParser(string filename)
        {
            ObjParser parser = new ObjParser();
            Model model = parser.ParseFile(filename);
            TestClientGameObject go = new TestClientGameObject(model);
            GameObjects.Add(go);
        }
    }   
}
