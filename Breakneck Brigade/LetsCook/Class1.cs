using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tao.Ode;
using Tao.FreeGlut;
using Tao.OpenGl;
using Tao.Glfw;

namespace LetsCook
{
    class Class1
    {
        static IntPtr world; // a physics world Ode.dWorldID
        static IntPtr ball;    // an ball de.dBodyID
        static double radius = 0.2, mass = 1.0;// radius(m)、weight(kg) 

        // Simulation loop
        void simLoop(int pause)
        {
            Ode.dVector3 pos; //　position 
            Ode.dMatrix3 R; // rotation matrix 　
            Ode.dWorldStep(world, 0.05);// Step a simulation world, time step is 0.05 [s]
            pos = Ode.dBodyGetPosition(ball);// Get a position
            R = Ode.dBodyGetRotation(ball);// Get a rotation

            // Ode.dsSetColor(1.0,0.0,0.0);  // Set color  (red, green, blue) value is from 0 to 1.0
            // Ode.dsDrawSphere(pos,R,radius);　　// Draw a sphere
        }


        float[] xyz = new float[3] { 0.0f, -3.0f, 1.0f };   // View position (x, y, z [m])
        float[] hpr = new float[3] { 90.0f, 0.0f, 0.0f };  // View direction （head, pitch, roll[°]）

        // Start function 
        void start()
        {
            // dsSetViewpoint (xyz,hpr);　　// Set a view point
        }

        static double t = 0, dt = 0, t_old = 0;

        static void Main(string[] args)
        {
            double x0 = 0.0, y0 = 0.0, z0 = 1.0; // initial position of an ball[m]
            Ode.dMass m1 = new Ode.dMass(); // mass parameter

            Ode.dInitODE();      // Initialize ODE
            world = Ode.dWorldCreate();  // Create a dynamic world
            Ode.dWorldSetGravity(world, 0, 0, -0.001);// Set gravity（x, y, z)

            //　Create a ball
            ball = Ode.dBodyCreate(world); //  Crate a rigid body
            Ode.dMassSetZero(ref m1);// Initialize mass parameters
            Ode.dMassSetSphereTotal(ref m1, mass, radius);// Calculate a mass parameter

            Ode.dBodySetMass(ball, ref m1); // Set a mass parameter
            Ode.dBodySetPosition(ball, x0, y0, z0);//　Set a position(x, y, z)

            // Simulation loop
            // argc, argv are argument of main function. Window size is  352 x 288 [pixel]
            // fn is a structure of drawstuff
            // for drawstuff
            // dsFunctions fn;　//　drawstuff structure
            // fn.version = DS_VERSION;　  // the version of the drawstuff
            // fn.start = &start;　　　　　      　// start function
            // fn.step = &simLoop;　　        // step function
            // fn.command = NULL;　　　　   // no command function for keyboard
            // fn.stop 　　 = NULL;　　　　     // no stop function
            // fn.path_to_textures = "../../drawstuff/textures";　//　path to the texture
            // dsSimulationLoop (argc,argv,352,288,&fn);
            
            /* Init GLFW */
            Glfw.glfwInit();

            Glfw.glfwOpenWindowHint(Glfw.GLFW_DEPTH_BITS, 16);

            int window = Glfw.glfwOpenWindow(400, 400, 0, 0, 0 ,0, 0, 0, Glfw.GLFW_WINDOW_NO_RESIZE);
            if (window != 0)
            {
                Glfw.glfwTerminate();
                return;
            }

            // Glfw.glfwSetFramebufferSizeCallback(window, reshape);
            // Glfw.glfwSetKeyCallback(window, key_callback);
            // Glfw.glfwMakeContextCurrent(window);
            Glfw.glfwSwapInterval(1);

            // Glfw.glfwGetFramebufferSize(window, &width, &height);
            // reshape(window, width, height);

            Glfw.glfwSetTime(0.0);
            Gl.glClearColor(0.55f, 0.55f, 0.55f, 0.0f);
            Gl.glShadeModel(Gl.GL_FLAT);

            /* Main loop */
            for (; ; )
            {
                /* Timing */
                t = Glfw.glfwGetTime();
                dt = t - t_old;
                t_old = t;

                /* Draw one frame */

                /* Swap buffers */
                // Glfw.glfwSwapBuffers(window);
                // Glfw.glfwPollEvents();

                /* Check if we are still running */
                //if (Glfw.glfwWindowShouldClose(window))
                //    break;
            }

            Glfw.glfwTerminate();
            Ode.dWorldDestroy(world);// Destroy the world 　
            Ode.dCloseODE();           // Close ODE
        }
    }
}
