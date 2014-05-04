using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.Ode;

namespace DeCuisine
{
    /// <summary>
    /// Helper class to make physics calculations easier
    /// </summary>
    static class Physics
    {
        public struct VelocityStruct
        {
            public double x, y, z;
            public VelocityStruct(double x1, double y1, double z1)
            {
                x = x1;
                y = y1;
                z = z1;
            }
            public static VelocityStruct operator +(VelocityStruct a, VelocityStruct b)
            {
                return new VelocityStruct(a.x + b.x, a.y + b.y, a.z + b.z);
            }
            public static Ode.dVector3 operator +(VelocityStruct a, Ode.dVector3 b)
            {
                return new Ode.dVector3(a.x + b.X, a.y + b.Y, a.z + b.Z );
            }
            public static Ode.dVector3 operator +( Ode.dVector3 b, VelocityStruct a) // reverse for ease of use
            {
                return new Ode.dVector3(a.x + b.X, a.y + b.Y, a.z + b.Z );
            }
        }

        public static int FloorHeight = 0;
        public static VelocityStruct Gravity;

        public static VelocityStruct AddVelocities(VelocityStruct vel1, VelocityStruct vel2)
        {
            return vel1+vel2;
        }


        /// <summary>
        /// Return a new position with gravity applied
        /// </summary>
        public static Ode.dVector3 ApplyGravity(Ode.dVector3 position)
        {
            return Physics.Gravity+position;
        }


        /// <summary>
        /// Apply the given force to the position and return the new position
        /// </summary>
        public static Ode.dVector3 ApplyForce(VelocityStruct force, Ode.dVector3 postion)
        {
            return force + postion;
        }

        /// <summary>
        /// Simulate all forces on the 
        /// </summary>
        /// <param name="force">Arrary of all forces that are acting on the object</param>
        /// <param name="postion"></param>
        /// <returns></returns>
        public static Ode.dVector3 Simulate(VelocityStruct[] force, Ode.dVector3 position)
        {
            return position;
        }

        public static void SetGravity(double acc)
        {
            Physics.Gravity = new VelocityStruct(0, 0, acc);
        }
    }
}
