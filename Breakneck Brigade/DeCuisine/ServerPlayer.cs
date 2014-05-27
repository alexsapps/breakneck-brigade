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
        private const float JUMPSPEED = 100;
        private const float THROWSCALER = 500;
        private const float SHOOTSCALER = 1000; // A boy can dream right?
        private const float DASHSCALER = 1500;
        private const float HOLDDISTANCE = 40.0f;
        private const float LINEOFSIGHTSCALAR = 50;
        private const int DASHTIME = 90; // seconds * 30. 
        private int dashTicks { get; set; }
        public string Name { get; set; }

        private bool isFalling { get; set; }
        private bool canJump { get; set; }
        private Vector3 lastVelocity { get; set; }
        private Vector3 lastDashVelocity { get; set; }

        protected override GeometryInfo getGeomInfo() { return BB.GetPlayerGeomInfo(); }

        /// <summary>
        /// Gets the ServerObject this player is currently looking at.
        /// </summary>


        ServerGameObject _lookingAt;
        public ServerGameObject LookingAt
        {
            get { return _lookingAt; }
            set
            {
                if(_lookingAt != null)
                    _lookingAt.Removed -= _lookingAt_Removed;
                _lookingAt = value;
                if(_lookingAt != null)
                    _lookingAt.Removed += _lookingAt_Removed;
            }
        }
        void _lookingAt_Removed(object sender, EventArgs e)
        {
            Debug.Assert(sender == LookingAt);
            LookingAt = null;
        }

        public override GameObjectClass ObjectClass { get { return GameObjectClass.Player; } }
        public Client Client { get; private set; }
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
           
            public HandInventory(ServerGameObject toHold) 
            {
                this.Held = toHold;
            }
        }
        public Dictionary<string,HandInventory> Hands;

        public ServerPlayer(ServerGame game, Vector3 position, Client client) 
            : base(game)
        {
            base.AddToWorld(position);
            this.Body.AngularFactor = new Vector3(0, 0, 0);
            this.Client = client;
            this.Hands = new Dictionary<string, HandInventory>();
            HandInventory tmp = new HandInventory(null);
            this.Hands.Add("left", tmp);
            this.Hands.Add("right", tmp);
            this.dashTicks = 0; // don't start dashing
        }

        public override void Serialize(BinaryWriter stream)
        {
            base.Serialize(stream);

            int lookingAtId = -1;
            if(this.LookingAt != null)
                lookingAtId = this.LookingAt.Id;
            stream.Write(lookingAtId);
        }


        /// <summary>
        /// Update the current stream. Mostly position but orientation and
        /// incline are handled here
        /// </summary>
        /// <param name="stream"></param>
        public override void UpdateStream(BinaryWriter stream)
        {
            base.UpdateStream(stream);
        }

        /// <summary>
        /// Moves a player relative to his or her current position
        /// </summary>
        public void Move(Vector3 vel)
        {
            this.dashTicks = 0;
            this.Body.LinearVelocity = new Vector3(vel.X, this.Body.LinearVelocity.Y + vel.Y, vel.Z);
            this.Body.ActivationState = ActivationState.ActiveTag;
        }

        /// <summary>
        /// Moves a player relative to his or her current position
        /// </summary>
        public void Move(float x, float y, float z)
        {
            this.dashTicks = 0;
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
                this.Hands["left"] = new HandInventory(obj); // book keeping to keep track
                ((ServerIngredient)obj).LastPlayerHolding = this;
            }
        }

        public void Dash()
        {
            this.dashTicks = DASHTIME;
            SousChef.Vector4 imp = new SousChef.Vector4(0.0f, 0.0f, -1.0f);
            Matrix4 rotate = Matrix4.MakeRotateYDeg(-this.Orientation) * Matrix4.MakeRotateXDeg(-this.Incline);
            imp = rotate * imp;

            this.Body.LinearVelocity = new Vector3(imp.X, imp.Y, imp.Z) * ServerPlayer.DASHSCALER;
            this.lastDashVelocity = this.Body.LinearVelocity;
        }

        /// <summary>
        /// Throw an object from the passed in hand
        /// </summary>
        /// <param name="hand"></param>
        public void Throw(string hand, float orientation, float incline, float scalar)
        {
            SousChef.Vector4 imp = new SousChef.Vector4(0.0f, 0.0f, -1.0f);
            Matrix4 rotate = Matrix4.MakeRotateYDeg(-orientation) * Matrix4.MakeRotateXDeg(-incline);
            imp = rotate * imp; 
            imp *= scalar;

            // Cause you can shoot oranges now. Why the fuck not? 
            if (this.Hands[hand].Held == null)
            {
                var ingSpawn = new Vector3(
                    this.Position.X + (float)Math.Sin(this.Orientation * Math.PI / 180.0f) * HOLDDISTANCE,
                    this.Position.Y + (float)Math.Sin(this.Incline * Math.PI / 180.0f) * HOLDDISTANCE * -1,
                    this.Position.Z + (float)Math.Cos(this.Orientation * Math.PI / 180.0f) * HOLDDISTANCE * -1);
                var tmp = new ServerIngredient(this.Game.Config.Ingredients["orange"], Game, ingSpawn);
                tmp.Body.LinearVelocity = new Vector3(imp.X, imp.Y, imp.Z) * ServerPlayer.SHOOTSCALER;
                return;
            }

            this.Hands[hand].Held.Body.Gravity = this.Game.World.Gravity;
            this.Hands[hand].Held.Body.LinearVelocity = new Vector3(imp.X, imp.Y, imp.Z) * ServerPlayer.THROWSCALER;
            this.Hands[hand] = new HandInventory(null); //clear the hands
        }

        /// <summary>
        /// Attempts to eject a cooker it is looking at.
        /// </summary>
        public void AttemptToEjectCooker()
        {
            if(this.LookingAt != null && this.LookingAt.ObjectClass == GameObjectClass.Cooker)
            {
                ((ServerCooker)this.LookingAt).Eject();
            }
        }

        public void AttemptToCook()
        {
            if (this.LookingAt != null && this.LookingAt.ObjectClass == GameObjectClass.Cooker)
            {
                ((ServerCooker)this.LookingAt).Cook();
            }
        }

        /// <summary>
        /// Causes player to jump into the air.
        /// </summary>
        public void Jump()
        {
            if (this.canJump || Game.MultiJump)
            {
                this.Move(this.Body.LinearVelocity.X, ServerPlayer.JUMPSPEED, this.Body.LinearVelocity.Z);
                this.canJump = false;
                this.isFalling = false;
            }
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
                this.Hands["left"].Held.Position = new Vector3(this.Position.X + (float)Math.Sin(
                    this.Orientation * Math.PI / 180.0f) * HOLDDISTANCE, 
                    this.Position.Y + (float)Math.Sin(this.Incline * Math.PI / 180.0f) * -HOLDDISTANCE, 
                    this.Position.Z + (float)Math.Cos(this.Orientation * Math.PI / 180.0f) * -HOLDDISTANCE);
            }

            // Check what the player is looking at
            Vector3 start = new Vector3
                (
                    this.Position.X + (float)Math.Sin(this.Orientation * Math.PI / 180.0f) * HOLDDISTANCE * 2,
                    this.Position.Y + (float)Math.Sin(this.Incline * Math.PI / 180.0f) * HOLDDISTANCE * -2,
                    this.Position.Z + (float)Math.Cos(this.Orientation * Math.PI / 180.0f) * HOLDDISTANCE * -2
               );

            Vector3 end = new Vector3(start.X, start.Y, start.Z) * ServerPlayer.LINEOFSIGHTSCALAR;

            CollisionWorld.ClosestRayResultCallback collisionCallback = new CollisionWorld.ClosestRayResultCallback(start, end);
            this.Game.World.RayTest(start, end, collisionCallback);
            if (collisionCallback.HasHit)
            {
                this.LookingAt = (ServerGameObject)collisionCallback.CollisionObject.CollisionShape.UserObject;
            }
            else
            {
                this.LookingAt = null;
            }
            if (dashTicks > 0)
            {
                CheckDashing();
            }

        }

        private void CheckDashing()
        {
            dashTicks--;
            if (dashTicks == 0)
            {
                // stop dashing
                this.Body.LinearVelocity = -lastDashVelocity;
            }
        }

        public override void Remove()
        {
            base.Remove();

            Client.Disconnect();
        }

        public override string ToString()
        {
            return Name ?? "Player" + Id;
        }
    }
}
