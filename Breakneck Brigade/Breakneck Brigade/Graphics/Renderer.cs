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
        public static TextRenderer TextRenderer;
        private ModelParser parser;
        private const DebugMode DEBUG_MODE = DebugMode.OFF;
        private const string RESOURCES_XML_PATH = "res\\resources.xml";
        private const string CROSSHAIR_MODEL_NAME = "crosshair";
        public const string BLANKQUAD_MODEL_NAME = "blankQuad";
        public const string FONT_TEXTURE = "fontWhite.tga";
        private int padding = 5;
        private int spacing = 10;

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

        public IList<ClientGameObject>      GameObjects { get; set; }
        public IList<AParticleSpawner>      ParticleSpawners { get; set; }

        private     Matrix4         WorldTransform;
        private     Camera          Camera;
        public      static int      WindowWidth    = 0;
        public      static int      WindowHeight   = 0;
        private     const float     _desiredWidth   = 1024.0f;
        private     const float     _desiredHeight  = 768.0f;
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
                    while (firstPass.ReadToNextSibling("texture"))
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

                        if (ii != numberOfTextures - 1)
                        {
                            reader.ReadEndElement();
                            reader.ReadToNextSibling("texture");
                        }
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

                        if (ii != numberOfModels - 1)
                        {
                            reader.ReadEndElement();
                            reader.ReadToNextSibling("model");
                        }
                    }
                }
            
            }
            //2D UI setup
            VBO crosshairVBO = new VBO();
            float[] crosshairIndices = {
                                  0, 1, 2, 3, 4, 5,
                                  6, 7, 8, 9, 10, 11
                              };
            float[] crosshairData = {
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
            crosshairVBO.Indices.AddRange(crosshairIndices);
            crosshairVBO.Data.AddRange(crosshairData);
            crosshairVBO.LoadData();
            TexturedMesh crosshairMesh = new TexturedMesh() { VBO = crosshairVBO, Texture = Renderer.Textures["white.tga"] };
            Model crosshairModel = new Model(CROSSHAIR_MODEL_NAME);
            crosshairModel.Meshes.Add(crosshairMesh);
            crosshairModel.ModelMatrix = Matrix4.MakeTranslationMat(.5f, 0.5f, 0.0f) * Matrix4.MakeScalingMat(.10f, .10f*_desiredRatio, 1.0f);

            Models.Add(crosshairModel.Name, crosshairModel);

            VBO blankQuad = VBO.MakeCornerQuadWithUVCoords(new float[] { 0, 1 }, new float[] { 1, 1 }, new float[] { 1, 0 }, new float[] { 0, 0 });
            blankQuad.LoadData();
            TexturedMesh blankQuadMesh = new TexturedMesh() { VBO = blankQuad, Texture = Renderer.Textures["fontWhite.tga"] };
            Model blankQuadModel = new Model(BLANKQUAD_MODEL_NAME);
            blankQuadModel.ModelMatrix = Matrix4.MakeTranslationMat(.5f, .5f, 0.0f) * Matrix4.MakeScalingMat(10f, 10f, 1f);
            blankQuadModel.Meshes.Add(blankQuadMesh);

            Models.Add(blankQuadModel.Name, blankQuadModel);

            TextRenderer = new TextRenderer();
        }

        public void Render(LocalPlayer lp)
        {
            setViewport();
            
            //Always clear both color and depth
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            
            prep3D(lp);
            render3D();
            prep2D();
            Render2D(lp);

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
            float[] ambientColor = { .1f, .1f, .1f, 1 };
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
            //Gl.glEnable(Gl.GL_CULL_FACE);
            //For basic polygons. Only draws front faces
            Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);
            //What shading model to use for rendering Gl prims
            Gl.glShadeModel(Gl.GL_SMOOTH);
            //Turn on texturing
            Gl.glEnable(Gl.GL_TEXTURE_2D);
            //Turn on depth test
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glDepthFunc(Gl.GL_LEQUAL);

            //Disable transparency by default
            Renderer.disableTransparency();

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
            Glfw.glfwOpenWindow((int) _desiredWidth, (int) _desiredHeight, 8, 8, 8, 8, 16, 0, Glfw.GLFW_WINDOW);
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
            
            Glfw.glfwGetWindowSize(out WindowWidth, out WindowHeight);
            var actualRatio = (float) WindowWidth / (float) WindowHeight;

            Gl.glViewport(0, 0, WindowWidth, WindowHeight);
        }

        private void prep2D()
        {
            float aspect = (float)WindowWidth / (float)WindowHeight;
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

        private void Render2D(LocalPlayer player)
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
            Renderer.enableTransparency();
            //Models[BLANKQUAD_MODEL_NAME].Render();
            this.DrawGoals(5, padding);
            this.DrawHeld(500, 30);
            if (player != null)
            {
                this.DrawRecipe(player.SelectedRecipe, 5, 500);
            }

            this.DrawTime((int)WindowWidth / 2, (int)WindowHeight - 20);
            this.DrawScores(5, (int)WindowHeight - 20);


            Renderer.disableTransparency();
            //Models["iceCream"].Render();

            Gl.glPopAttrib();
        }

        private void DrawGoals(int xPos, int yPos)
        {
            var goalCache = Program.game != null ? Program.game.Goals : null;
            if (goalCache != null)
            {
                goalCache = goalCache.ToList(); //make it a cache, so we can enumerate safely
                foreach (IngredientType it in goalCache)
                {
                    TextRenderer.printToScreen(xPos, yPos, it.FriendlyName, .75f, .75f);
                    yPos += spacing + padding;
                }
                TextRenderer.printToScreen(xPos, yPos, "\"Make these items!\"", .75f, .75f);
                yPos += spacing + padding;
                TextRenderer.printToScreen(xPos, yPos, "The master chef sez", .75f, .75f);
            }
        }

        private void DrawHeld(int xPos, int yPos)
        {
            if (Program.game != null && Program.game.LiveGameObjects != null)
            {
                string lookingAt;
#if PROJECT_WORLD_BUILDING
                string info = "";
#endif
                if (Program.game.LiveGameObjects.ContainsKey(Program.game.LookatId))
                {
                    ClientGameObject lookedAtObject = Program.game.LiveGameObjects[Program.game.LookatId];
                    lookingAt = lookedAtObject.ModelName;
#if PROJECT_WORLD_BUILDING
                    // append id if we are building the world
                    lookingAt += " " + Program.game.LookatId +
                        " At position ";
                    info = +(int)lookedAtObject.Position.X + " " +
                        (int)lookedAtObject.Position.Y + " " + (int)lookedAtObject.Position.Z + 
                        " scaled at " + (int)lookedAtObject.Sides[0] + " " + (int)lookedAtObject.Sides[1] + " " +
                        (int)lookedAtObject.Sides[2];
#endif
                    if(lookedAtObject is ClientCooker)
                    {
                        ClientCooker lookedAtCooker = (ClientCooker)lookedAtObject;
                        lookingAt += " (" + lookedAtCooker.Team.Name + ")";
                        string ingedientList = "- ";
                        foreach(ClientIngredient ingredient in lookedAtCooker.Contents)
                        {
                            ingedientList += ingredient.ModelName + " ";
                        }

                        ingedientList += "-";

                        //yPos -= (spacing + padding);
                        TextRenderer.printToScreen(xPos, yPos - (spacing + padding), ingedientList, .75f, .75f);
                    }

                    // lookingAt = lookedAtObject.ToString();
                }
                else
                {
                    lookingAt = "nothing";
                }
#if PROJECT_WORLD_BUILDING
                TextRenderer.printToScreen(500, 40, "Looking at: " + lookingAt, .75f, .75f);
                TextRenderer.printToScreen(500, 15, info, .75f, .75f);
#else
                TextRenderer.printToScreen(xPos, yPos, "Looking at: " + lookingAt, .75f, .75f);
#endif
                
            }
        }

        /// <summary>
        /// Draws the timer
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        private void DrawTime(int xPos, int yPos)
        {
            if (Program.game != null)
            {
                TextRenderer.printToScreen(xPos, yPos, "Time: " + Program.game.GameTime.ToString(), .75f, .75f);
            }
        }

        /// <summary>
        /// Draw team scores.
        /// </summary>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        private void DrawScores(int xPos, int yPos)
        {
            string header;
            if (Program.lobbyState.MyTeam != null)
                header = "Team: " + Program.lobbyState.MyTeam.Name;
            else
                header = "Team Scores:";
            TextRenderer.printToScreen(xPos, yPos, header, .75f, .75f);
            yPos -= (spacing + padding);
            TextRenderer.printToScreen(xPos, yPos, "-----------", .75f, .75f);
            yPos -= (spacing + padding);
            if (Program.lobbyState != null && Program.lobbyState.Teams != null)
            {
                foreach (ClientTeam team in Program.lobbyState.Teams.Values.ToList())
                {
                    TextRenderer.printToScreen(xPos, yPos, team.Name + ": " + team.Score.ToString(), .75f, .75f);
                    yPos -= (spacing + padding);
                }
            }
        }

        /// <summary>
        /// Draw current cookbook page.
        /// </summary>
        /// <param name="selectedRecipe"></param>
        /// <param name="xPos"></param>
        /// <param name="yPos"></param>
        private void DrawRecipe(Recipe selectedRecipe, int xPos, int yPos)
        {
            int padding = 5;
            int spacing = 10;
            if (selectedRecipe != null)
            {
                TextRenderer.printToScreen(xPos, yPos, selectedRecipe.FriendlyName, .75f, .75f);
                yPos -= (spacing + padding);
                TextRenderer.printToScreen(xPos, yPos, "----------", .75f, .75f);
                yPos -= (spacing + padding);
                TextRenderer.printToScreen(xPos, yPos, "USABLE COOKERS:", .75f, .75f);
                yPos -= (spacing + padding);
                foreach (CookerType cooker in selectedRecipe.UsableCookers)
                {
                    TextRenderer.printToScreen(xPos, yPos, cooker.FriendlyName, .75f, .75f);
                    yPos -= (spacing + padding);
                }

                TextRenderer.printToScreen(xPos, yPos, "----------", .75f, .75f);
                yPos -= (spacing + padding);
                //TextRenderer.printToScreen(xPos, yPos, "REQURIED:" , .75f, .75f);
                //yPos -= (spacing + padding);
                foreach (RecipeIngredient ingredient in selectedRecipe.Ingredients)
                {
                    for( int i = 0; i < ingredient.nCount; i++)
                    {
                        TextRenderer.printToScreen(xPos, yPos, ingredient.Ingredient.FriendlyName, .75f, .75f);
                        yPos -= (spacing + padding);
                    }
                }

                TextRenderer.printToScreen(xPos, yPos, "----------", .75f, .75f);
                yPos -= (spacing + padding);
                TextRenderer.printToScreen(xPos, yPos, "OPTIONAL:", .75f, .75f);
                yPos -= (spacing + padding);
                foreach (RecipeIngredient ingredient in selectedRecipe.Ingredients)
                {
                    for (int i = 0; i < ingredient.nOptional; i++)
                    {
                        TextRenderer.printToScreen(xPos, yPos, ingredient.Ingredient.FriendlyName, .75f, .75f);
                        yPos -= (spacing + padding);
                    }
                }
            }
        }

        ModelTimer modelTimer = null;

        private void render3D()
        {
            //Game objects
            if (GameObjects != null)
                foreach (ClientGameObject cgo in GameObjects)
                {
                    if(modelTimer != null)
                        modelTimer.start(cgo.Model);
                    
                    cgo.Render();

                    if(modelTimer != null)
                        modelTimer.stop();
                }

            Renderer.enableTransparency();
            //Particles
            if(ParticleSpawners != null)
            {           
                foreach (AParticleSpawner ps in ParticleSpawners)
                {
                    ps.Render();
                }

            }
            Renderer.disableTransparency();
            //DebugPicking();

            CurrentDrawMode = -1;
        }

        private static void DebugPicking()
        {
            // Debug triangles for picking
            Gl.glDisable(Gl.GL_CULL_FACE);
            Gl.glColor3f(1.0f, 0f, 0f);
            Gl.glBegin(Gl.GL_TRIANGLES);
            Gl.glVertex3f(ClientPlayer.start.X, ClientPlayer.start.Y, ClientPlayer.start.Z);
            Gl.glVertex3f(ClientPlayer.start.X, ClientPlayer.start.Y + 5, ClientPlayer.start.Z);
            Gl.glVertex3f(ClientPlayer.start.X, ClientPlayer.start.Y, ClientPlayer.start.Z + 5);
            Gl.glEnd();

            Gl.glColor3f(1.0f, 1.0f, 0f);
            Gl.glBegin(Gl.GL_TRIANGLES);
            Gl.glVertex3f(ClientPlayer.end.X, ClientPlayer.end.Y, ClientPlayer.end.Z);
            Gl.glVertex3f(ClientPlayer.end.X, ClientPlayer.end.Y + 5, ClientPlayer.end.Z);
            Gl.glVertex3f(ClientPlayer.end.X, ClientPlayer.end.Y, ClientPlayer.end.Z + 5);
            Gl.glEnd();
            Gl.glDisable(Gl.GL_CULL_FACE);
            Gl.glColor3f(1.0f, 1.0f, 1.0f);
        }

        public static void enableTransparency()
        {
            //Blending for transparency
            Gl.glEnable(Gl.GL_BLEND);
            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
            Gl.glEnable(Gl.GL_ALPHA_TEST);
            Gl.glDepthMask(false);
        }

        public static void disableTransparency()
        {
            Gl.glDisable(Gl.GL_BLEND);
            Gl.glDisable(Gl.GL_ALPHA_TEST);
            Gl.glDepthMask(true);
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
