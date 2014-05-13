using BulletSharp;
using SousChef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCuisine
{
    class ServerPlayer : ServerGameObject
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Player; } }
        protected override GeometryInfo getGeomInfo() { return new GeometryInfo() { Mass = 40, Shape = GeomShape.Box, Sides = new float[] { 6.0f, 6.0f, 6.0f } }; }
        public Client Client { get; private set; }
        private bool isFalling { get; set; }
        private bool canJump { get; set; }
        private Vector3 lastVelocity { get; set; }
        private float JUMPSPEED = 100;

        // TEST CODE
        bool flag = false;
        //

        public struct HandInventory
        {
            public ServerGameObject Held;
            public Point2PointConstraint Joint;
            public HandInventory(ServerGameObject toHold, Point2PointConstraint joint) //, Joint joint)
            {
                this.Held = toHold;
                this.Joint = joint;
            }
        }
        public HandInventory LeftHand;
        public HandInventory RightHand;

        public ServerPlayer(ServerGame game, Vector3 position, Client client) 
            : base(game)
        {
            base.AddToWorld(position);
            this.Client = client;
            this.LeftHand = new HandInventory(null, null);
            this.RightHand = new HandInventory(null, null);
        }

        public override void Serialize(BinaryWriter stream)
        {
            base.Serialize(stream);
            //stream.Write(a,b);
            //stream.Write(c,d);
        }

        public override void UpdateStream(BinaryWriter stream)
        {
            base.UpdateStream(stream);
            //stream.Write(c,d);
        }

        /// <summary>
        /// Moves a player relative to his or her current position
        /// </summary>
        public void Move(Vector3 vel)
        {
            this.Body.LinearVelocity = new Vector3(vel.X, this.Body.LinearVelocity.Y + vel.Y, vel.Z);
            this.Body.ActivationState = ActivationState.ActiveTag;
        }

        /// <summary>
        /// Moves a player relative to his or her current position
        /// </summary>
        public void Move(float x, float y, float z)
        {
            this.Body.LinearVelocity = new Vector3(x, this.Body.LinearVelocity.Y + y, z);
            this.Body.ActivationState = ActivationState.ActiveTag;
        }

        public override void OnCollide(ServerGameObject obj)
        {
            if (obj.ObjectClass == GameObjectClass.Ingredient
                && !flag)
            {
                // TODO: Try different joints to see what works best.

                //obj.Position = new Vector3(this.Position.X, this.Position.Y + 10, this.Position.Z);
                Vector3 pivotA = new Vector3(0,0,0);
                Vector3 pivotB = new Vector3(0,0,0);
               
                //Vector3 axisInA = Vector3.Zero;
                //Vector3 axisInB = new Vector3(0, 1, 0); // new Vector
                //HingeConstraint joint = new HingeConstraint(this.Body, obj.Body, pivotA, pivotB, axisInA, axisInB);
                //joint.SetLimit(-(float)Math.PI / 8, (float)Math.PI / 8);
                //SliderConstraint joint = new SliderConstraint(this.Body, obj.Body, Matrix.Identity, Matrix.Identity, true);
                //joint.LowerLinLimit = 1f;
                //joint.UpperLinLimit = 1f;
                //joint.UpperAngularLimit = 1f;
                //joint.LowerAngularLimit = 1f;
                obj.Body.Gravity = Vector3.Zero;
                //joint.SetLimit(0f, 0f);
                //Generic6DofConstraint joint = new Generic6DofConstraint(this.Body, obj.Body, Matrix.Identity, Matrix.Identity, true);
                //joint.LinearLowerLimit = new Vector3(0, 1, 1);
                //joint.LinearUpperLimit = new Vector3(0, 1, 1);
                //joint.AngularUpperLimit = new Vector3(0, 1, 1);
                //joint.AngularLowerLimit = new Vector3(0, 1, 1);
                Point2PointConstraint joint = new Point2PointConstraint(this.Body, obj.Body, pivotA, pivotB);
                //joint.SetParam()
                Game.World.AddConstraint(joint, true);
                flag = true;
                this.LeftHand = new HandInventory(obj, joint); // book keeping to keep track

                
            }

        }

        /// <summary>
        /// Throw an object from the passed in hand
        /// </summary>
        /// <param name="hand"></param>
        public void Throw(string hand, float x, float y, float z)
        {
            HandInventory tmp;
            if (hand == "left" && this.LeftHand.Held != null)
                tmp = this.LeftHand;
            else if (this.RightHand.Held != null)
                tmp = this.RightHand;
            else
                return;

            this.Game.World.RemoveConstraint(tmp.Joint);
            tmp.Held.Body.Gravity = this.Game.World.Gravity;
            tmp.Held.Body.LinearVelocity = new Vector3(x, 40, z);
            tmp.Held.Position = new Vector3(tmp.Held.Position.X, tmp.Held.Position.Y + 20, tmp.Held.Position.Z);
            this.flag = false;

        }
        public void Jump()
        {
            if (this.canJump || Game.MultiJump)
            {
                this.Move(this.Body.LinearVelocity.X, this.JUMPSPEED, this.Body.LinearVelocity.Z);
                this.canJump = false;
                this.isFalling = false;
            }
        }

        private void makeJoint(string hand, ServerGameObject obj)
        {
            
        }

        protected override void updateHook()
        {
            if (this.Body.LinearVelocity.Y < 0)
            {
                this.isFalling = true;
            }
            if (this.isFalling && this.Body.LinearVelocity.Y >= 0)
            {
                this.canJump = true;
            }
        }

        public override void Remove()
        {
            base.Remove();

            Client.Disconnect();
        }
    }
}
