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
        public VelocityStruct FRICTION = new VelocityStruct(-.1,-.1,.0); // No terminal velocity
        public int FloorHeight = 0;
        public VelocityStruct Gravity;

        /// <summary>
        /// Makes a totally bug free Physics engine. Acc should be the ABSOLUTE force for gravity.
        /// </summary>
        /// <param name="acc"></param>
        /// <param name="frameRate"></param>
        public Physics(double acc, double frameRate)
        {
            this.Gravity = new VelocityStruct(0, 0, -Math.Abs(acc * (frameRate / 1000)));
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
        public static Vector3 ApplyForce(VelocityStruct force, Vector3 postion)
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
            
            Vector3 newPosition = Physics.ApplyForce(obj.Velocity, new Vector3(obj.Position));
            if (newPosition.z < 1)
            {
                // hit the ground, set position to the ground
                newPosition.z = 0;
            }
            return newPosition.convertOde();
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
        public DistanceJoint HoldObj(ServerGameObject obj1, ServerGameObject obj2)
        {
            Vector3 origin = new Vector3(obj1.Position);
            Vector3 holdDistance = new Vector3(10,10,0);
            obj2.PhysicsOn = false;
            return new DistanceJoint(origin, holdDistance);
        }
    }

    /// <summary>
    /// Makes a joint as if they were held together by a pole
    /// </summary>
    public class DistanceJoint 
    {
        Vector3 _origin;
        public Vector3 Origin { get { return this._origin; } set { this._origin = value; moveOrigin(value); } }
        Vector3 HoldPoint, BobMaxDistance, BobRate, HoldDistance;

       
       /// <summary>
       /// Make a DistanceJoint from origin to hold point. Bob rate 
       /// is how fast the object bobs around in the x y and z direction
       /// </summary>
       /// <param name="origin"></param>
       /// <param name="holdPoint"></param>
       /// <param name="bobRate"></param>
        public DistanceJoint(Vector3 origin, Vector3 holdDistance )
        {
            this.HoldPoint = new Vector3(origin.x + holdDistance.x, origin.y + holdDistance.y, origin.z + holdDistance.z);
            this.HoldDistance = holdDistance;

            this.Origin = origin;
            //this.BobMaxDistance = bobMaxDistance;
            //this.BobRate = bobRate;

        }

        /// <summary>
        /// Update the joint to be at the position passed in, returns where the object at the hold 
        /// point should be
        /// </summary>
        public Vector3 Update(Vector3 pos)
        {
            this.Origin = pos;
            return HoldPoint;
        }

        /// <summary>
        /// Moves everything associated with this joint
        /// </summary>
        /// <param name="value"></param>
        private void moveOrigin(Vector3 value)
        {
            this._origin = value;
            this.HoldPoint = this._origin + this.HoldDistance; 
        }
    }

    public class VelocityStruct
    {
        public double x, y, z;
        public VelocityStruct(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public static VelocityStruct operator +(VelocityStruct a, VelocityStruct b)
        {
            return new VelocityStruct(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static Vector3 operator +(VelocityStruct a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }
    }

    public class Vector3
    {
        public double x, y, z;
        public Vector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public Vector3(Ode.dVector3 vec)
        {
            this.x = vec.X;
            this.y = vec.Y;
            this.z = vec.Z;
        }
        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }
        public static Vector3 operator +(Vector3 b, VelocityStruct a) // reverse for ease of use
        {
            return new Vector3(a.x + b.x, a.y + b.x, a.z + b.x);
        }
        public static Vector3 operator +(Vector3 a, double[] b)
        {
            if(b.Length != 3)
                throw new Exception();
            return new Vector3(a.x + b[0], a.y + b[1], a.z + b[2]);
        }

        public Ode.dVector3 convertOde()
        {
            return new Ode.dVector3(this.x, this.y, this.z);
        }
    }

    
}
