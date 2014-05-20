using BulletSharp;
using SousChef;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCuisine
{
    class ServerPlayer : ServerGameObject
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Player; } }
        protected override GeometryInfo getGeomInfo() { return new GeometryInfo() { Mass = 40, Shape = GeomShape.Box, Sides = new float[] { 6.0f, 6.0f, 6.0f }, Friction = 1.0f, Restitution = 0.2f }; }
        public Client Client { get; private set; }
        private bool isFalling { get; set; }
        private bool canJump { get; set; }
        private Vector3 lastVelocity { get; set; }
        private const float JUMPSPEED = 100;
        private const float THROWSPEED = 300;
        private const float SHOOTSCALER = 10; // A boy can dream right?
        private const float HOLDDISTANCE = 40.0f;

        public override int SortOrder { get { return 10000; } } /* must be sent after ingredients, because players can be holding ingredients */

        public class HandInventory
        {
            private ServerGameObject _held;
            public ServerGameObject Held
            {
                get
                {
                    return _held;
                }
                set
                {
                    if (_held != null)
                        _held.Removed -= Held_Removed;
                    _held = value;
                    if (_held != null)
                        _held.Removed += Held_Removed;
                }
            }

            void Held_Removed(object sender, EventArgs e)
            {
                Debug.Assert(sender == Held);
                Held = null;
            }
           
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
            base.OnCollide(obj);

            if (obj.ObjectClass == GameObjectClass.Ingredient
                && this.Hands["left"].Held == null)
            {
                obj.Body.Gravity = Vector3.Zero;
                this.Hands["left"] = new HandInventory(obj, null); // book keeping to keep track
            }

        }


        public void Dash()
        {
            SousChef.Vector4 imp = new SousChef.Vector4(0.0f, 0.0f, -ServerPlayer.THROWSPEED);
            Matrix4 rotate = Matrix4.MakeRotateYDeg(-this.Orientation) * Matrix4.MakeRotateXDeg(-this.Incline);
            imp = rotate * imp;

            this.Body.LinearVelocity = new Vector3(imp.X, imp.Y, imp.Z) * 3;
        }
        /// <summary>
        /// Throw an object from the passed in hand
        /// </summary>
        /// <param name="hand"></param>
        public void Throw(string hand, float orientation, float incline)
        {
            SousChef.Vector4 imp = new SousChef.Vector4(0.0f, 0.0f, -ServerPlayer.THROWSPEED);
            Matrix4 rotate = Matrix4.MakeRotateYDeg(-orientation) * Matrix4.MakeRotateXDeg(-incline);
            imp = rotate * imp;

            // Cause you can shoot oranges now. Why the fuck not? 
            if (this.Hands[hand].Held == null)
            {
                var ingSpawn = new Vector3(this.Position.X + (float)Math.Sin(this.Orientation * Math.PI / 180.0f) * HOLDDISTANCE,
                                                                                                    this.Position.Y + (float)Math.Sin(this.Incline * Math.PI / 180.0f) * HOLDDISTANCE * -1,
                                                                                                    this.Position.Z + (float)Math.Cos(this.Orientation * Math.PI / 180.0f) * HOLDDISTANCE * -1);
                var tmp = new ServerIngredient(this.Game.Config.Ingredients["orange"], Game, ingSpawn);
                tmp.Body.LinearVelocity = new Vector3(imp.X, imp.Y, imp.Z) * ServerPlayer.SHOOTSCALER;
                return;
            }

            this.Hands[hand].Held.Body.Gravity = this.Game.World.Gravity;
            this.Hands[hand].Held.Body.LinearVelocity = new Vector3(imp.X, imp.Y, imp.Z);
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
                this.Hands["left"].Held.Position = new Vector3(this.Position.X + (float)Math.Sin(   this.Orientation * Math.PI / 180.0f) * HOLDDISTANCE, 
                                                                                                    this.Position.Y + (float)Math.Sin(this.Incline * Math.PI / 180.0f) * HOLDDISTANCE * -1, 
                                                                                                    this.Position.Z + (float)Math.Cos(this.Orientation * Math.PI / 180.0f) * HOLDDISTANCE * -1);
            }
        }

        public override void Remove()
        {
            base.Remove();

            Client.Disconnect();
        }

        
    }
}
