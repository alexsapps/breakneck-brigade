using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tao.Ode;

namespace LetsCook
{
    class Cooker
    {
        /// <summary>
        /// Get or set the name of the object.
        /// </summary>
        public String Name { get; set; }

        /// <summary>
        /// Get or set the pointer to the rigid body.
        /// </summary>
        public IntPtr Body { get; private set; }

        /// <summary>
        /// Gets or sets the list of ingredients currently held by this cooker.
        /// </summary>
        public List<Ingredient> CurrentlyHeldIngredients { get; set; }

        public Cooker(String name, IntPtr world, double radius, double mass, float x, float y, float z)
        {
            this.Name = name;
            this.CurrentlyHeldIngredients = new List<Ingredient>();
            this.CreateRigidBody(world, radius, mass, x, y, z);
        }

        /// <summary>
        /// Creates a rigid body for this Cooker
        /// </summary>
        /// <param name="world"></param>
        /// <param name="radius"></param>
        /// <param name="mass"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        private void CreateRigidBody(IntPtr world, double radius, double mass, float x, float y, float z)
        {
            // Sphere-shaped?
            this.Body = Ode.dBodyCreate(world); //  Crate a rigid body
            Ode.dMass m1 = new Ode.dMass(); // mass parameter
            Ode.dMassSetZero(ref m1);// Initialize mass parameters
            Ode.dMassSetSphereTotal(ref m1, mass, radius);// Calculate a mass parameter
            Ode.dBodySetMass(this.Body, ref m1); // Set a mass parameter
            Ode.dBodySetPosition(this.Body, x, y, z);//　Set a position(x, y, z)
        }

        public bool CheckCooking()
        {
            // Check if ingredients within this cooker match with a possible combination. Use parser here. 
            return false;
        }
    }
}
