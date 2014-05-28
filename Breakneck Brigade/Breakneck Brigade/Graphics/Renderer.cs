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
        private const string CROSSHAIR_MODEL_NAME = "crosshair";

        private Stopwatch  _stopwatch = new Stopwatch();
        private Stopwatch  _stopwatch2 = new Stopwatch();
        private BBStopwatch _stopwatch3 = new BBStopwatch(Program.clientConsole);
        public float       secondsPerFrame = 0.0f;
        public float       secondsPerFrame2 = 0.0f;

        public static int                           CurrentBoundTexture = -1;
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
        private     const float     _desiredWidth = 1024.0f;
        private     const float     _desiredHeight = 768.0f;
        private     const float     _desiredRatio = _desiredWidth/_desiredHeight;

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
                int numberOfTextures = 0;
                using (XmlReader firstPass = XmlReader.Create(resFile))
                {
                    firstPass.ReadToFollowing("resources");
                    firstPass.ReadToFollowing("textures");
                    if (firstPass.ReadToDescendant("texture"))
                    {
                        numberOfTextures++;
                    }
                    while (firstPass.ReadToNextSibling("textures"))
                    {
                        numberOfTextures++;
                    }

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
                    //Extra textures
                    reader.ReadToFollowing("resources");
                    reader.ReadToFollowing("textures");
                    reader.ReadToDescendant("texture");
                    for (int ii = 0; ii < numberOfTextures; ii++)
                    {
                        XmlReader modelSubtree = reader.ReadSubtree();

                        modelSubtree.ReadToDescendant("filename");
                        string filename = modelSubtree.ReadElementContentAsString() + ".tga";

                        Texture texture;
                        if (Renderer.Textures.ContainsKey(filename))
                        {
                            texture = Renderer.Textures[filename];
                        }
                        else
                        {
                            texture = new Texture(filename);
                            Renderer.Textures[filename] = texture;
                        }
                    }
                    //Models + mtls + associated textures
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
            //2D UI setup
            VBO crosshairVBO = new VBO();
            float[] indices = {
                                  0, 1, 2, 3, 4, 5,
                                  6, 7, 8, 9, 10, 11
                              };
            float[] data = {
                               //Horiz
                                -0.1f,  0.01f,  0,    //V0
                                    0,     0, -1,    //N0
                                    0,     1,        //T0
                                 0.1f, -0.01f,  0,    //V2
                                    0,     0, -1,    //N2
                                    1,     0,        //T2
                                 0.1f,  0.01f,  0,    //V1
                                    0,     0, -1,    //N1
                                    1,     1,        //T1
                                -0.1f,  0.01f,  0,    //V0
                                    0,     0, -1,    //N0
                                    0,     1,        //T0
                                -0.1f, -0.01f,  0,    //V3
                                    0,     0, -1,    //N3
                                    0,     1,        //T3
                                 0.1f, -0.01f,  0,    //V2
                                    0,     0, -1,    //N2
                                    1,     0,        //T2
                                //Vert
                                -0.01f,  0.1f,  0,    //V0
                                    0,     0, -1,    //N0
                                    0,     1,        //T0
                                 0.01f, -0.1f,  0,    //V2
                                    0,     0, -1,    //N2
                                    1,     0,        //T2
                                 0.01f,  0.1f,  0,    //V1
                                    0,     0, -1,    //N1
                                    1,     1,        //T1
                                -0.01f,  0.1f,  0,    //V0
                                    0,     0, -1,    //N0
                                    0,     1,        //T0
                                -0.01f, -0.1f,  0,    //V3
                                    0,     0, -1,    //N3
                                    0,     1,        //T3
                                 0.01f, -0.1f,  0,    //V2
                                    0,     0, -1,    //N2
                                    1,     0,        //T2

                            };
            crosshairVBO.Indices.AddRange(indices);
            crosshairVBO.Data.AddRange(data);
            crosshairVBO.LoadData();
            TexturedMesh crosshairMesh = new TexturedMesh() { VBO = crosshairVBO, Texture = Renderer.Textures["white.tga"] };
            Model crosshairModel = new Model(CROSSHAIR_MODEL_NAME);
            crosshairModel.Meshes.Add(crosshairMesh);
            crosshairModel.ModelMatrix = Matrix4.MakeTranslationMat(.5f, 0.5f, 0.0f) * Matrix4.MakeScalingMat(.10f, .10f*_desiredRatio, 1.0f);


            Models.Add(crosshairModel.Name, crosshairModel);

        }

        public void Render(LocalPlayer lp)
        {
            setViewport();
            
            //Always clear both color and depth
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            
            prep3D(lp);
            render3D();
            prep2D();
            render2D();

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

            float[] position1 = { 1000, 2000, 0, 1 };
            float[] position2 = { 1000, 2000, 0, 1};
            float[] ambientColor = { .2f, .2f, .2f, 1 };
            //float[] ambientColor = { 0, 0, 0, 1 };
            float[] diffuseColor = { 1, 1, 1, 1 };
            float[] specularColor = { 1, 1, 1, 1 };
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, position1);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_AMBIENT, ambientColor);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, diffuseColor);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPECULAR, specularColor);
            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_POSITION, position2);
            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_AMBIENT, ambientColor);
            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_DIFFUSE, diffuseColor);
            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_SPECULAR, specularColor);
            Gl.glEnable(Gl.GL_LIGHT0);
            Gl.glEnable(Gl.GL_LIGHT1);

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
            //Turn on depth test
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glDepthFunc(Gl.GL_LEQUAL);

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
            Glfw.glfwOpenWindowHint(Glfw.GLFW_WINDOW_NO_RESIZE, Gl.GL_TRUE);
            Glfw.glfwOpenWindow((int) _desiredWidth, (int) _desiredHeight, 0, 0, 0, 8, 16, 0, Glfw.GLFW_WINDOW);
            Glfw.glfwSwapInterval(0);

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
            var actualRatio = (float) _windowWidth / (float) _windowHeight;

            Gl.glViewport(0, 0, _windowWidth, _windowHeight);
        }

        private void prep2D()
        {
            float aspect = (float)_windowWidth / (float)_windowHeight;
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Glu.gluOrtho2D(0, 1, 0, 1);

            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();


            Gl.glColor3f(1, 1, 1);

            Gl.glPushAttrib(Gl.GL_ENABLE_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            Gl.glDisable(Gl.GL_DEPTH_TEST);
            Gl.glDisable(Gl.GL_CULL_FACE);
            //Gl.glDisable(Gl.GL_TEXTURE_2D);
            Gl.glDisable(Gl.GL_LIGHTING);
            //Gl.glTranslatef(0.375f, 0.375f, 0.0f);

            //Gl.glDisable(Gl.GL_DEPTH_TEST);
        }

        private void prep3D(LocalPlayer lp)
        {
            Camera.Update(lp);
            Camera.Render();
        }

        private void render2D()
        {
            /*
            Gl.glPushMatrix();
            Gl.glBegin(Gl.GL_QUADS);
                Gl.glVertex3f(-.50f, .50f, 0.0f);
                Gl.glVertex3f(-.50f, -.50f, 0.0f);
                Gl.glVertex3f(.50f, -.50f, 0.0f);
                Gl.glVertex3f(.50f, .50f, 0.0f);
            Gl.glEnd();
            Gl.glPopMatrix();
             */
            Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);
            Models[CROSSHAIR_MODEL_NAME].Render();
            //Models["iceCream"].Render();

            Gl.glPopAttrib();
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
