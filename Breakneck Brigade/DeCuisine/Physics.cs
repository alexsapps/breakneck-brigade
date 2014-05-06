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
    class Physics
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
        public  VelocityStruct FRICTION = new VelocityStruct(-.1,-.1,.0); // No terminal velocity
        public int FloorHeight = 0;
        public VelocityStruct Gravity;

        /// <summary>
        /// Makes a totally bug free Physics engine. Acc should be the ABSOLUTE force for gravity.
        /// </summary>
        /// <param name="acc"></param>
        /// <param name="frameRate"></param>
        public Physics(double acc, int frameRate)
        {
            this.Gravity = new VelocityStruct(0, 0, -Math.Abs(acc / (frameRate / 1000)));
            Console.WriteLine(this.Gravity);
        }


        public static VelocityStruct AddVelocities(VelocityStruct vel1, VelocityStruct vel2)
        {
            return vel1 + vel2;
        }


        /// <summary>
        /// Apply gravity acceleration to the position of at the current velocity
        /// </summary>
        public VelocityStruct ApplyGravity(VelocityStruct currentVelocity)
        {
            return this.Gravity + currentVelocity;
        }


        /// <summary>
        /// Apply the given force to the position and return the new position
        /// </summary>
        public static Ode.dVector3 ApplyForce(VelocityStruct force, Ode.dVector3 postion)
        {
            return force + postion;
        }

        /// <summary>
        /// Add the force to the given velocity vector
        /// </summary>
        public static VelocityStruct AddForce(VelocityStruct force, VelocityStruct  velocity)
        {
            return force + velocity;
        }

        /// <summary>
        /// Simulate the object i.e. Applay gravity, friction, bounces etc. 
        /// </summary>
        /// <returns>New position of the passed in object</returns>
        public Ode.dVector3 Simulate(ServerGameObject obj)
        {

            obj.Velocity = this.ApplyGravity(obj.Velocity);
            Ode.dVector3 newPosition = Physics.ApplyForce(obj.Velocity, obj.Position);
            if (newPosition.Z < 1)
            {
                // hit the ground, set position to the ground
                newPosition.Z = 0;
            }
            return newPosition;
        }

        /// <summary>
        /// Sets the Acceleration of the z direction(Also know as graivty)
        /// </summary>
        /// <param name="acc"></param>
        public void SetGravity(double acc)
        {
            this.Gravity = new VelocityStruct(0, 0, acc);
        }

        public VelocityStruct ApplyFriction(VelocityStruct vel)
        {
            VelocityStruct newVel = new VelocityStruct(0, 0, 0);
            if (vel.x != 0)
            {
                newVel.x = (vel.x + this.FRICTION.x) > .1 ? vel.x + this.FRICTION.x : 0;
            }
            if (vel.y != 0)
            {
               newVel.y = (vel.y + this.FRICTION.y) > .1 ? vel.y + this.FRICTION.y : 0;
            }
            return newVel + vel;

        }
    }
}
