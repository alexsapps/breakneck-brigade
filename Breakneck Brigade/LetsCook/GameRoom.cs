using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using Tao.Ode;

namespace LetsCook
{
    class GameInstance
    {
        /// <summary>
        /// The physics-modeled world.
        /// </summary>
        private IntPtr world;

        /// <summary>
        /// Gets or sets the frequencies of the room.
        /// </summary>
        public double Frequency { get; set; }

        /// <summary>
        /// Gets or sets the players in the room.
        /// </summary>
        public List<Player> Players { get; private set; }

        /// <summary>
        /// Gets or sets the players in the room.
        /// </summary>
        public List<Ingredient> Ingredients { get; private set; }

        /// <summary>
        /// Gets or sets the players in the room.
        /// </summary>
        public List<Cooker> Cookers { get; private set; }

        /// <summary>
        /// Get or set the list of other rigid bodies in the room that does not deal with gameplay. (i.e. Walls)
        /// </summary>
        public List<IntPtr> OtherRigidBodies { get; set; }
        
        /// <summary>
        /// Creates a new game room.
        /// TODO: What kind of parameters are needed? Number of players, ingredients placement, etc.
        /// </summary>
        public GameInstance()
        {
            this.Frequency = 0.05;
            this.Initialize();
        }

        /// <summary>
        /// Initializes the room to the beginning of a game.
        /// </summary>
        public void Initialize()
        {
            // Initialize variables
            Players = new List<Player>();
            Ingredients = new List<Ingredient>();
            Cookers = new List<Cooker>();

            // Create physics world
            Ode.dInitODE();      // Initialize ODE
            world = Ode.dWorldCreate();  // Create a dynamic world
            Ode.dWorldSetGravity(world, 0, 0, -0.001);// Set gravity（x, y, z)

            // Generate Walls of room. 
            double[] sides = new double[]{0.5,0.5,1.0}; // length of edges
            IntPtr wall = Ode.dBodyCreate(world);

            Ode.dMass m = new Ode.dMass(); // mass parameter
            Ode.dMassSetBox(ref m, 5.0, sides[0], sides[1], sides[2]);
            Ode.dBodySetMass(wall, ref m);
            Ode.dBodySetPosition(wall, 0, 2, 1);

            // Spawn players and objects. Get from parser
        }

        public void Update()
        {
            DateTime start = DateTime.Now;

            // Apply inputs

            // Apply physics
            Ode.dWorldStep(world, this.Frequency); // Step a simulation world

            // Sleep
            DateTime end = DateTime.Now;
            if ((start - end).TotalSeconds > Frequency)
                throw new Exception("Update step is longer than frequency.");
            else
            {
                Thread.Sleep((int)((Frequency - (start - end).TotalSeconds) * 1000)); // Sleep until next step
            }
        }

        public void Finalize()
        {
            Ode.dWorldDestroy(world);  // Destroy the world 　
            Ode.dCloseODE();           // Close ODE
        }

        private void CreateObject(double x, double y, double z, double mass)
        {
            IntPtr newRigidBody;    // an ball de.dBodyID
            double x0 = 0.0, y0 = 0.0, z0 = 1.0; // initial position of an ball[m]
            double radius = 0.2;

            newRigidBody = Ode.dBodyCreate(world); //  Crate a rigid body
            Ode.dMass m1 = new Ode.dMass(); // mass parameter
            Ode.dMassSetZero(ref m1);// Initialize mass parameters
            Ode.dMassSetSphereTotal(ref m1, mass, radius);// Calculate a mass parameter
            Ode.dBodySetMass(newRigidBody, ref m1); // Set a mass parameter
            Ode.dBodySetPosition(newRigidBody, x0, y0, z0);//　Set a position(x, y, z)
        }

        private void GetObjectInfo()
        {
            // Ode.dVector3 pos; //　position 
            // Ode.dMatrix3 R; // rotation matrix 　
            // pos = Ode.dBodyGetPosition(ball);// Get a position
            // R = Ode.dBodyGetRotation(ball);// Get a rotation
        }
    }
}
