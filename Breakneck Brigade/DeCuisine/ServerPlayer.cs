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
        private const float HOLDDISTANCE = 10.0f;
        private const float LINEOFSIGHTSCALAR = 200;
        private const float RAYSTARTDISTANCE = 10;
#if PROJECT_DEBUG
        private const int DASHTIME = 15;
#else
        private const int DASHTIME = 5; // seconds * 30. 
#endif
        private int dashTicks { get; set; }
        private int dashCool { get; set; }
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

        public float EyeHeight { get; private set; }
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
            this.EyeHeight = 15.0f;
            this.Body.AngularFactor = new Vector3(0, 0, 0);
            this.Client = client;
            this.Hands = new Dictionary<string, HandInventory>();
            HandInventory tmp = new HandInventory(null);
            this.Hands.Add("left", tmp);
            this.Hands.Add("right", tmp);
            this.dashTicks = 0; // don't start dashing
            this.dashCool = 0;
        }

        public override void Serialize(BinaryWriter stream)
        {
            base.Serialize(stream);
            stream.Write(this.Client.Team.Name);
            int lookingAtId = -1;
            if(this.LookingAt != null)
                lookingAtId = this.LookingAt.Id;
            stream.Write(lookingAtId);
            stream.Write(this.start);
            stream.Write(this.end);
            stream.Write(this.EyeHeight);
        }


        /// <summary>
        /// Update the current stream. Mostly position but orientation and
        /// incline are handled here
        /// </summary>
        /// <param name="stream"></param>
        public override void UpdateStream(BinaryWriter stream)
        {
            base.UpdateStream(stream);
            int lookingAtId = -1;
            if (this.LookingAt != null)
                lookingAtId = this.LookingAt.Id;
            stream.Write(lookingAtId);
            stream.Write(this.start);
            stream.Write(this.end);
            stream.Write(this.EyeHeight);
            if (this.Hands["left"].Held != null)
                stream.Write(this.Hands["left"].Held.Id);
            else
                stream.Write(-1);
        }

        /// <summary>
        /// Moves a player relative to his or her current position
        /// </summary>
        public void Move(Vector3 vel)
        {
            if (this.dashTicks != 0)
                return;
            this.Body.LinearVelocity = new Vector3(vel.X, this.Body.LinearVelocity.Y + vel.Y, vel.Z);
            this.Body.ActivationState = ActivationState.ActiveTag;
        }

        /// <summary>
        /// Moves a player relative to his or her current position
        /// </summary>
        public void Move(float x, float y, float z)
        {
            if (this.dashTicks != 0)
                return;
            this.Body.LinearVelocity = new Vector3(x, this.Body.LinearVelocity.Y + y, z);
            this.Body.ActivationState = ActivationState.ActiveTag;
        }

        public override void OnCollide(ServerGameObject obj)
        {
            base.OnCollide(obj);
        }

        public void Dash()
        {
            if (this.dashCool != 0)
                return; // no dash until cooldown is done.
            this.dashTicks = DASHTIME;
            SousChef.Vector4 imp = new SousChef.Vector4(0.0f, 0.0f, -1.0f);
            Matrix4 rotate = Matrix4.MakeRotateYDeg(-this.Orientation) * Matrix4.MakeRotateXDeg(-this.Incline);
            imp = rotate * imp;

            this.Body.LinearVelocity = new Vector3(imp.X, imp.Y, imp.Z) * ServerPlayer.DASHSCALER;
            this.lastDashVelocity = this.Body.LinearVelocity;
#if PROJECT_DEBUG
            this.dashCool = 0;
#else
            this.dashCool = 5 * dashTicks;
#endif
        }

        /// <summary>
        /// Throw an object from the passed in hand
        /// </summary>
        /// <param name="hand"></param>
        public void HandleClick(string hand, float orientation, float incline, float scalar)
        {
            if (this.Hands[hand].Held == null && this.LookingAt != null && 
                this.LookingAt.ObjectClass == GameObjectClass.Ingredient)
                this.PickUpObject((ServerIngredient)this.LookingAt);
            else
            {
                SousChef.Vector4 imp = new SousChef.Vector4(0.0f, 0.0f, -1.0f);
                Matrix4 rotate = Matrix4.MakeRotateYDeg(-orientation) * Matrix4.MakeRotateXDeg(-incline);
                imp = rotate * imp;
                imp *= scalar;

                // find out where your cross hairs are.
                var crossPos = new Vector3(
                        this.Position.X + (float)Math.Sin(this.Orientation * Math.PI / 180.0f) * HOLDDISTANCE,
                        this.Position.Y + this.EyeHeight + (float)Math.Sin(this.Incline * Math.PI / 180.0f) * HOLDDISTANCE * -1,
                        this.Position.Z + (float)Math.Cos(this.Orientation * Math.PI / 180.0f) * HOLDDISTANCE * -1);

                // Is this to get around a bug? Why would you want to do this...
                var _hand = this.Hands[hand];
                var held = _hand.Held;

                // Cause you can shoot oranges now. Why the fuck not? 
                if (held == null)
                {
                    var tmp = new ServerIngredient(this.Game.Config.Ingredients["orange"], Game, crossPos);
                    tmp.Body.LinearVelocity = new Vector3(imp.X, imp.Y, imp.Z) * ServerPlayer.SHOOTSCALER;
                    return;
                }

                // Throw object
                held.Position = crossPos;
                held.Body.Gravity = this.Game.World.Gravity;
                held.Body.LinearVelocity = new Vector3(imp.X, imp.Y, imp.Z) * ServerPlayer.THROWSCALER;
                if (held.Body.LinearVelocity.X == 0 && held.Body.LinearVelocity.Y == 0 && held.Body.LinearVelocity.Z == 0)
                    Console.WriteLine("WHAT THE FUCK");
                _hand.Held = null;
                MarkDirty();
            }

        }

        private void PickUpObject(ServerIngredient obj)
        {
            obj.Body.Gravity = Vector3.Zero;
            this.Hands["left"] = new HandInventory(obj); // book keeping to keep track
            obj.LastPlayerHolding = this;
            MarkDirty();
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
            //DEV
            Program.WriteLine("Attempting to cook");
            if (this.LookingAt != null && this.LookingAt.ObjectClass == GameObjectClass.Cooker)
            {
                Program.WriteLine("raycast worked, but can it cook?");
                var tmp = ((ServerCooker)this.LookingAt).Cook();
                if (tmp != null)
                    Program.WriteLine("Cook was successful, cooked a " + tmp.Type.Name);
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
                this.Hands["left"].Held.Position = new Vector3(
                    this.Position.X + (float)(Math.Sin(this.Orientation * Math.PI / 180.0f) * HOLDDISTANCE),
                    this.Position.Y + this.EyeHeight * 0.75f + (float)Math.Sin(this.Incline * Math.PI / 180.0f) * -HOLDDISTANCE, // TODO: Have the hold logic be a lot better
                    this.Position.Z + (float)(Math.Cos(this.Orientation * Math.PI / 180.0f) * -HOLDDISTANCE));
            }

            // Check what the player is looking at
            Vector3 start = new Vector3
                (
                    this.Position.X + (float)(Math.Sin(this.Orientation * Math.PI / 180.0f) * RAYSTARTDISTANCE), //Math.Sin(this.Incline * Math.PI / 180.0f) *
                    this.Position.Y + this.EyeHeight + (float)Math.Sin(this.Incline * Math.PI / 180.0f) * -RAYSTARTDISTANCE,
                    this.Position.Z + (float)(Math.Cos(this.Orientation * Math.PI / 180.0f) * -RAYSTARTDISTANCE) //Math.Sin(this.Incline * Math.PI / 180.0f) * 
               );

            Vector3 end = new Vector3
                (
                    start.X + (float)(Math.Sin(this.Orientation * Math.PI / 180.0f) * LINEOFSIGHTSCALAR), //Math.Sin(this.Incline * Math.PI / 180.0f) * 
                    start.Y + (float)Math.Sin(this.Incline * Math.PI / 180.0f) * -LINEOFSIGHTSCALAR,
                    start.Z + (float)(Math.Cos(this.Orientation * Math.PI / 180.0f) * -LINEOFSIGHTSCALAR) //Math.Sin(this.Incline * Math.PI / 180.0f) * 
                );
            CollisionWorld.ClosestRayResultCallback raycastCallback = new CollisionWorld.ClosestRayResultCallback(start, end);
            this.Game.World.RayTest(start, end, raycastCallback);
            if (raycastCallback.HasHit)
            {
                this.LookingAt = (ServerGameObject)raycastCallback.CollisionObject.CollisionShape.UserObject;
            }
            else
            {
                this.LookingAt = null;
            }

            this.start = start;
            this.end = end;

            // Check if player needs to stop dashing
            if (dashTicks > 0)
                CheckDashing();
            if (this.dashCool > 0)
                this.dashCool--;

        }

        Vector3 start = new Vector3(), end = new Vector3();

        private void CheckDashing()
        {
            this.dashTicks--;
            if (dashTicks == 0)
            {
                this.Body.LinearVelocity = new Vector3(0, 0, 0);
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
