using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.Ode;
using SousChef;

namespace DeCuisine
{

    /// <summary>
    /// Helper class to make physics calculations easier
    /// </summary>
    class Physics
    {
        public Vector4 FRICTION = new Vector4(-.1,-.1,.0); // No terminal velocity
        public int FloorHeight = 0;
        public Vector4 Gravity;

        /// <summary>
        /// Makes a totally bug free Physics engine. Acc should be the ABSOLUTE force for gravity.
        /// </summary>
        /// <param name="acc"></param>
        /// <param name="frameRate"></param>
        public Physics(double acc, double frameRate)
        {
            this.Gravity = new Vector4(0, 0, -Math.Abs(acc * (frameRate / 1000)));
            Console.WriteLine(this.Gravity);
        }


        public static Vector4 AddVelocities(Vector4 vel1, Vector4 vel2)
        {
            return vel1 + vel2;
        }



        /// <summary>
        /// Apply gravity acceleration to the position of at the current velocity
        /// </summary>
        public Vector4 ApplyGravity(Vector4 currentVelocity)
        {
            return this.Gravity + currentVelocity;
        }

        /// <summary>
        /// Apply the given force to the position and return the new position
        /// </summary>
        public static Vector4 ApplyForce(Vector4 force, Vector4 postion)
        {
            return force + postion;
        }

        /// <summary>
        /// Add the force to the given velocity vector
        /// </summary>
        public static Vector4 AddForce(Vector4 force, Vector4  velocity)
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
            
            Vector4 newPosition = Physics.ApplyForce(obj.Velocity, obj.Position.ConvertToVector4());
            if (newPosition[2] < 1)
            {
                // hit the ground, set position to the ground
                newPosition[2] = 0;
            }
            return Utils.ConvertToOde(newPosition);
        }

        /// <summary>
        /// Sets the Acceleration of the z direction(Also know as graivty)
        /// </summary>
        /// <param name="acc"></param>
        public void SetGravity(double acc)
        {
            this.Gravity = new Vector4(0, 0, acc);
        }

        public Vector4 ApplyFriction(Vector4 vel)
        {
            Vector4 newVel = new Vector4(0, 0, 0);
            if (vel[0] != 0)
            {
                newVel[0] = (vel[0] + this.FRICTION[0]) > .1 ? vel[0] + this.FRICTION[0] : 0;
            }
            if (vel[1] != 0)
            {
               newVel[1] = (vel[1] + this.FRICTION[1]) > .1 ? vel[1] + this.FRICTION[1] : 0;
            }
            return newVel + vel;
        }

        public DistanceJoint HoldObj(ServerGameObject obj1, ServerGameObject obj2)
        {
            Vector4 origin = obj1.Position.ConvertToVector4();
            Vector4 holdDistance = new Vector4(10,10,0);
            obj2.PhysicsOn = false;
            return new DistanceJoint(origin, holdDistance);
        }
    
    }

    /// <summary>
    /// Makes a joint as if they were held together by a pole
    /// </summary>
    public class DistanceJoint
    {
        Vector4 _origin;
        public Vector4 Origin { get { return this._origin; } set { this._origin = value; moveOrigin(value); } }
        Vector4 HoldPoint, BobMaxDistance, BobRate, HoldDistance;


        /// <summary>
        /// Make a DistanceJoint from origin to hold point. Bob rate 
        /// is how fast the object bobs around in the x y and z direction
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="holdPoint"></param>
        /// <param name="bobRate"></param>
        public DistanceJoint(Vector4 origin, Vector4 holdDistance)
        {
            this.HoldPoint = new Vector4(origin[0] + holdDistance[0], origin[1] + holdDistance[1], origin[2] + holdDistance[2]);
            this.HoldDistance = holdDistance;

            this.Origin = origin;
            //this.BobMaxDistance = bobMaxDistance;
            //this.BobRate = bobRate;

        }

        /// <summary>
        /// Update the joint to be at the position passed in, returns where the object at the hold 
        /// point should be
        /// </summary>
        public Vector4 Update(Vector4 pos)
        {
            this.Origin = pos;
            return HoldPoint;
        }

        /// <summary>
        /// Moves everything associated with this joint
        /// </summary>
        /// <param name="value"></param>
        private void moveOrigin(Vector4 value)
        {
            this._origin = value;
            this.HoldPoint = this._origin + this.HoldDistance;
        }
    }
    
    public static class Utils
    {
        public static Ode.dVector3 ConvertToOde(this Vector4 vec)
        {
            return new Ode.dVector3(vec[0], vec[1], vec[2]);
        }

        public static Vector4 ConvertToVector4(this Ode.dVector3 vec)
        {
            return new Vector4(vec.X, vec.Y, vec.Z);
        }


    }
}
