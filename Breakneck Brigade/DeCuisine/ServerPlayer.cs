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
        private const float JUMPSPEED = 100;
        private const float THROWSPEED = 300;


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
        public Dictionary<string,HandInventory> Hands;

        public ServerPlayer(ServerGame game, Vector3 position, Client client) 
            : base(game)
        {
            base.AddToWorld(position);
            this.Client = client;
            this.Hands = new Dictionary<string, HandInventory>();
            HandInventory tmp = new HandInventory(null, null);
            this.Hands.Add("left", tmp);
            this.Hands.Add("right", tmp);
        }

        public override void Serialize(BinaryWriter stream)
        {
            base.Serialize(stream);
            //stream.Write(a,b);
            //stream.Write(c,d);
        }


        /// <summary>
        /// Update the current stream. Mostly position but orientation and
        /// incline are handled here
        /// </summary>
        /// <param name="stream"></param>
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
                && this.Hands["left"].Held == null)
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
                //Point2PointConstraint joint = new Point2PointConstraint(this.Body, obj.Body, pivotA, pivotB);
                //joint.SetParam()
                //Game.World.AddConstraint(joint, true);
                this.Hands["left"] = new HandInventory(obj, null); // book keeping to keep track
            }

        }

        /// <summary>
        /// Throw an object from the passed in hand
        /// </summary>
        /// <param name="hand"></param>
        public void Throw(string hand, float orientation, float incline)
        {
            if (this.Hands[hand].Held == null)
                return; //nothing in your hands

            SousChef.Vector4 imp = new SousChef.Vector4(0.0f, 0.0f, -ServerPlayer.THROWSPEED);
            Matrix4 rotate = Matrix4.MakeRotateYDeg(-orientation) * Matrix4.MakeRotateXDeg(-incline);
            imp = rotate * imp;

            //this.Game.World.RemoveConstraint(this.Hands[hand].Joint);
            this.Hands[hand].Held.Body.Gravity = this.Game.World.Gravity;
            this.Hands[hand].Held.Body.LinearVelocity = new Vector3(imp.X, imp.Y, imp.Z);
            this.Hands[hand].Held.Position = new Vector3(this.Position.X, this.Hands[hand].Held.Position.Y + 30, this.Position.Z);

            this.Hands[hand] = new HandInventory(null, null); //clear the hands
        }
        public void Jump()
        {
            if (this.canJump || Game.MultiJump)
            {
                this.Move(this.Body.LinearVelocity.X, ServerPlayer.JUMPSPEED, this.Body.LinearVelocity.Z);
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

            if (this.Hands["left"].Held != null)
            {
                // move the object in front of you
                this.Hands["left"].Held.Position = new Vector3(this.Position.X + 15, this.Position.Y + 15, this.Position.Z);
            }
        }

        public override void Remove()
        {
            base.Remove();

            Client.Disconnect();
        }
    }
}
