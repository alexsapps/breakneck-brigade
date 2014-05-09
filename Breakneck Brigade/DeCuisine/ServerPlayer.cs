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
        protected override GeometryInfo getGeomInfo() { return new GeometryInfo() { Mass = 40, Shape = GeomShape.Box, Sides = new float[] { 3.0f, 3.0f, 3.0f } }; }
        private bool isFalling { get; set; }
        private bool canJump { get; set; }
        private Vector3 lastVelocity { get; set; }
        private float JUMPSPEED = 100;

        public struct HandInventory
        {
            public ServerGameObject Held;
            //public Joint Joint;
            public HandInventory(ServerGameObject toHold) //, Joint joint)
            {
                this.Held = toHold;
                //this.Joint = joint;
            }
        }
        public HandInventory LeftHand;
        public HandInventory RightHand;
        private ServerGameObject toHold = null; // init to null as it's our flag TODO: Think smarter and don't do this

        public ServerPlayer(ServerGame game, Vector3 position) 
            : base(game)
        {
            base.AddToWorld(position);
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
            //if (obj.ObjectClass == GameObjectClass.Ingredient)
            //{
            //    this.toHold = obj;
            //}

        }

        public void Jump()
        {
            if (this.canJump)
            {
                this.Move(this.Body.LinearVelocity.X, this.JUMPSPEED, this.Body.LinearVelocity.Z);
                this.canJump = false;
                this.isFalling = false;
            }
        }

        private void makeJoint(string hand, ServerGameObject obj)
        {
            
            if (hand == "left")
            {
                // TODO: make logic to drop object if we have something in the hand already
                this.LeftHand = new HandInventory(obj);
            } else
            {
                this.RightHand = new HandInventory(obj);
            }
        }

        public override void Update()
        {
            base.Update();
            if (this.Body.LinearVelocity.Y < 0)
            {
                this.isFalling = true;
            }
            if (this.isFalling && this.Body.LinearVelocity.Y >= 0)
            {
                this.canJump = true;
            }
            //// Can't manipulate Ode in collide funtions, so a hacked flag has to do to make the joint
            //if (this.toHold != null)
            //{
            //    this.makeJoint("left", this.toHold);
            //    this.toHold = null;
            //}
        }
    }
}
