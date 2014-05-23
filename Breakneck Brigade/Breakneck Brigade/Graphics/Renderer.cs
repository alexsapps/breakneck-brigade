using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Tao.OpenGl;
using Tao.Glfw;
using System.Xml;
using System.IO;
using SousChef;

namespace Breakneck_Brigade.Graphics
{
    enum DebugMode
    {
        SNOWMAN,
        ORANGE,
        BOTH,
        OFF
    };
    class Renderer : IDisposable
    {
        private ModelParser parser;
        private const DebugMode DEBUG_MODE = DebugMode.OFF;
        private const string RESOURCES_XML_PATH = "res\\resources.xml";

        private Stopwatch  _stopwatch = new Stopwatch();
        private Stopwatch  _stopwatch2 = new Stopwatch();
        private BBStopwatch _stopwatch3 = new BBStopwatch(Program.clientConsole);
        public float       secondsPerFrame = 0.0f;
        public float       secondsPerFrame2 = 0.0f;

        public static int                           CurrentDrawMode = -1;
        /// <summary>
        /// A mapping of filename to Model
        /// </summary>
        public static Dictionary<string, Model>     Models      = new Dictionary<string, Model>();
        /// <summary>
        /// A mapping of filename to Texture
        /// </summary>
        public static Dictionary<string, Texture>   Textures    = new Dictionary<string, Texture>();

        /// <summary>
        /// The default checkerboard texture
        /// </summary>
        public static Texture   DefaultTexture;

        public IList<ClientGameObject>    GameObjects { get; set; }

        private     Matrix4         WorldTransform;
        private     Camera          Camera;
        private     int             _windowWidth    = 0;
        private     int             _windowHeight   = 0;
        private     int             _aspectX;
        private     int             _aspectY;
        private     float           _ratio = 1;
        private     bool            _getAspect      = false; // = true   (why not use ratio 1?)

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
            parser          = new ModelParser();
            WorldTransform  = new Matrix4();

            InitGLFW();
            InitGL();
            InitGlu();

            LoadResources();
        }

        public void LoadResources()
        {
            DefaultTexture = new Texture("default.tga");
            LoadModels();
        }

        public void LoadModels()
        {
            using (FileStream resFile = new FileStream(RESOURCES_XML_PATH, FileMode.Open))
            {
                int numberOfModels = 0;
                using (XmlReader firstPass = XmlReader.Create(resFile))
                {
                    firstPass.ReadToFollowing("models");
                    if (firstPass.ReadToDescendant("model"))
                    {
                        numberOfModels++;
                    }
                    while (firstPass.ReadToNextSibling("model"))
                    {
                        numberOfModels++;
                    }
                }
                resFile.Seek(0, SeekOrigin.Begin);
                using (XmlReader reader = XmlReader.Create(resFile))
                {
                    reader.ReadToFollowing("models");
                    reader.ReadToDescendant("model");
                    for(int ii = 0; ii < numberOfModels; ii++)
                    {
                        XmlReader modelSubtree = reader.ReadSubtree();

                        modelSubtree.ReadToDescendant("filename");
                        string filename = modelSubtree.ReadElementContentAsString();

                        modelSubtree.ReadToNextSibling("posX");
                        float posX = modelSubtree.ReadElementContentAsFloat();

                        modelSubtree.ReadToNextSibling("posY");
                        float posY = modelSubtree.ReadElementContentAsFloat();

                        modelSubtree.ReadToNextSibling("posZ");
                        float posZ = modelSubtree.ReadElementContentAsFloat();

                        modelSubtree.ReadToNextSibling("scaleX");
                        float scaleX = modelSubtree.ReadElementContentAsFloat();

                        modelSubtree.ReadToNextSibling("scaleY");
                        float scaleY = modelSubtree.ReadElementContentAsFloat();

                        modelSubtree.ReadToNextSibling("scaleZ");
                        float scaleZ = modelSubtree.ReadElementContentAsFloat();

                        modelSubtree.ReadToNextSibling("rotX");
                        float rotX = modelSubtree.ReadElementContentAsFloat();

                        modelSubtree.ReadToNextSibling("rotY");
                        float rotY = modelSubtree.ReadElementContentAsFloat();

                        modelSubtree.ReadToNextSibling("rotZ");
                        float rotZ = modelSubtree.ReadElementContentAsFloat();

                        Matrix4 modelMat = Matrix4.MakeTranslationMat(posX, posY, posZ);
                        modelMat *= Matrix4.MakeRotateZDeg(rotZ);
                        modelMat *= Matrix4.MakeRotateYDeg(rotY);
                        modelMat *= Matrix4.MakeRotateXDeg(rotX);
                        modelMat *= Matrix4.MakeScalingMat(scaleX, scaleY, scaleZ);

                        Model model = parser.ParseFile(filename);
                        modelMat *= model.ModelMatrix;
                        model.ModelMatrix = modelMat;

                        Models.Add(filename, model);

                        if(ii != numberOfModels - 1)
                            reader.ReadEndElement();
                            reader.ReadToNextSibling("model");
                    }
                }
            }
        }

        public void Render(LocalPlayer lp)
        {
            setViewport();
            
            //Always clear both color and depth
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT | Gl.GL_STENCIL_BUFFER_BIT);
            
            prep2D();
            render2D();
            prep3D(lp);
            render3D();

            _stopwatch3.Start();
            Glfw.glfwSwapBuffers();
            _stopwatch3.Stop(30, "slow swap buffers {0}");

            _stopwatch.Stop();
            _stopwatch2.Stop();
            secondsPerFrame += _stopwatch.ElapsedTicks / MathConstants.TICKS_PER_SECOND;
            secondsPerFrame /= 2;
            secondsPerFrame2 += _stopwatch2.ElapsedTicks / MathConstants.TICKS_PER_SECOND;
            secondsPerFrame2 /= 2;
            _stopwatch.Restart();
            // glfwSwapBuffers should implicitly call glfwPollEvents() by default
            //Glfw.glfwPollEvents();
        }

        /// <summary>
        /// Checks if the program should exit.
        /// </summary>
        /// <returns>Whether or not the program should exit</returns>
        public bool ShouldExit()
        {
            return Glfw.glfwGetWindowParam(Glfw.GLFW_OPENED) != Gl.GL_TRUE;
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
            //Enables manual setting of colors and materials of primatives (through glColor__, etc)
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            //Generates normals for objects which do not specify normals
            Gl.glEnable(Gl.GL_AUTO_NORMAL);
            //Turns on backface culling (culls surfaces you cannot see)
            Gl.glEnable(Gl.GL_CULL_FACE);
            //For basic polygons. Only draws front faces
            Gl.glPolygonMode(Gl.GL_FRONT, Gl.GL_FILL);
            //What shading model to use for rendering Gl prims
            //Gl.glShadeModel(Gl.GL_SMOOTH);
            //Turn on texturing
            Gl.glEnable(Gl.GL_TEXTURE_2D);

            //Optimizations
            Gl.glDisable(Gl.GL_DITHER);

            //VBO
            Gl.glEnableClientState(Gl.GL_VERTEX_ARRAY);
            Gl.glEnableClientState(Gl.GL_NORMAL_ARRAY);
            Gl.glEnableClientState(Gl.GL_TEXTURE_COORD_ARRAY);

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

        public Camera getCamera()
        {
            return Camera;
        }

        private void setViewport()
        {
            
            Glfw.glfwGetWindowSize(out _windowWidth, out _windowHeight);

            // force aspect ratio, use saved aspect ratio
            if (_getAspect)
            {
                _aspectX = _windowWidth;
                _aspectY = _windowHeight;
                _ratio = (float)_aspectY / (float)_aspectX;
                _getAspect = false;
            }

            // setting window viewport aspect to saved ratio
            if ((float)_windowHeight / (float)_windowWidth > _ratio)
            {
                Gl.glViewport(0, 0, (int)((float)_windowHeight / _ratio), _windowHeight);
            }
            else if ((float)_windowHeight / (float)_windowWidth < _ratio)
            {
                Gl.glViewport(0, 0, _windowWidth, (int)((float)_windowWidth * _ratio));
            }
            else
            {
                Gl.glViewport(0, 0, _windowWidth, _windowHeight);
            }
        }

        private void prep2D()
        {
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();

            Glu.gluOrtho2D(0.0f, _windowWidth, _windowHeight, 0.0f);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Gl.glTranslatef(0.375f, 0.375f, 0.0f);

            Gl.glDisable(Gl.GL_DEPTH_TEST);
        }

        private void prep3D(LocalPlayer lp)
        {
            Camera.Update(lp);
            Camera.Render();

            Gl.glDepthFunc(Gl.GL_LEQUAL);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
        }

        private void render2D()
        {
            /*
            Gl.glBegin(Gl.GL_LINES);
                Gl.glColor3f(0.0f, 0.0f, 0.0f);
                Gl.glVertex2d(0.5, 0.4);
                Gl.glVertex2d(0.5, 0.6);

                Gl.glVertex2d(0.4, 0.5);
                Gl.glVertex2d(0.6, 0.5);
            Gl.glEnd();
             * */
        }

        ModelTimer modelTimer = null;

        private void render3D()
        {
            if (GameObjects != null)
                foreach (ClientGameObject cgo in GameObjects)
                {
                    if(modelTimer != null)
                        modelTimer.start(cgo.Model);
                    
                    cgo.Render();

                    if(modelTimer != null)
                        modelTimer.stop();
                }
            CurrentDrawMode = -1;
        }

        public string GetModelTimerStatus()
        {
            if (modelTimer != null)
                return modelTimer.ToString();
            else
                return null;
        }

        public void ResetModelStatus()
        {
            modelTimer = new ModelTimer();
        }
    }

    class ModelTimer
    {
        public Dictionary<Model, TimedModel> timedModels = new Dictionary<Model, TimedModel>();
        public Stopwatch sw = new Stopwatch();

        Model current;

        public void start(Model model)
        {
            current = model;
            if (!timedModels.ContainsKey(model))
                timedModels.Add(current, new TimedModel() { model = current, ticks = 0, count = 0 });
            sw.Restart();
        }
        public void stop()
        {
            sw.Stop();
            var ticks = sw.ElapsedTicks;
            var timedModel = timedModels[current];
            timedModel.count++;
            timedModel.ticks += ticks;
            current = null;
        }

        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            foreach (var timedModel in timedModels.Values)
            {
                b.Append(timedModel.model.Name.PadRight(30));
                var timePerModel = timedModel.ticks / timedModel.count;
                b.Append(timePerModel.ToString().PadLeft(6));
                b.AppendLine();
            }
            return b.ToString();
        }
    }
    class TimedModel
    {
        public Model model;
        public int count;
        public long ticks;
    }
}
