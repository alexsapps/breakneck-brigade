using BulletSharp;
using SousChef;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SCVector4 = SousChef.Vector4;

namespace DeCuisine
{
    class ServerPlayer : ServerGameObject
    {        
        private const float JUMPSPEED = 100;
        private const float THROWSCALER = 500;
        private const float SHOOTSCALER = 1000; // A boy can dream right?
        private const float DASHSCALER = 1500;
        private const TimeSpan STUNTIME = new TimeSpan(0, 0, 0, 0, 50);

        //private const int STUNTIME = 300;
        private const float HOLDDISTANCE = 5.0f;
        private const float MAXVELOCITY = 380f;
#if PROJECT_WORLD_BUILDING
        private const float LINEOFSIGHTSCALAR = 100;//4000;
#else
        private const float LINEOFSIGHTSCALAR = 200;
#endif
        private const float RAYSTARTDISTANCE = 0;
#if PROJECT_DEBUG || PROJECT_WORLD_BUILDING
        private const int DASHTIME = 15;
#else
        private const int DASHTIME = 5; // seconds * 30. 
#endif
        private TimeSpan lastStunned = DateTime.Now.TimeOfDay;
        private bool isStunned = false;
        private int dashTicks { get; set; }
        private int dashCool { get; set; }
        public ServerTeam Team { get; set; }
        public string Name { get; set; }
        private Generic6DofConstraint constraint { get; set; }
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
            this.EyeHeight = 8.0f;
            this.constraint = null;
            this.Body.AngularFactor = new Vector3(0, 0, 0);
            this.Client = client;
            this.Hands = new Dictionary<string, HandInventory>();
            HandInventory tmp = new HandInventory(null);
            this.Hands.Add("left", tmp);
            this.Hands.Add("right", tmp);
            this.Team = null;
            this.dashTicks = 0; // don't start dashing
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
            if (this.dashTicks > 0 || this.isStunned)
                return;

            /*
            if (this.Body.LinearVelocity.Length() < MAXVELOCITY)
            {
                this.Body.ApplyCentralImpulse(vel);
            }
            */

            this.Body.LinearVelocity = new Vector3(vel.X, this.Body.LinearVelocity.Y + vel.Y, vel.Z);
            this.Body.ActivationState = ActivationState.ActiveTag;
        }

        /// <summary>
        /// Moves a player relative to his or her current position
        /// </summary>
        public void Move(float x, float y, float z)
        {
            this.Move(new Vector3(x, y, z));
        }

        public override void OnCollide(ServerGameObject obj)
        {
            base.OnCollide(obj);

            // If hit player with dash, stun them.
            if(obj is ServerPlayer)
            {
                ServerPlayer otherPlayer = (ServerPlayer)obj;
                if(this.dashTicks > 0 && this.Team != otherPlayer.Team)
                {
                    otherPlayer.Stun();
                }
            }
        }

        /// <summary>
        /// Perform an air dash.
        /// </summary>
        public void Dash()
        {
            if (this.dashCool > 0 || this.isStunned)
                return;
            this.dashTicks = DASHTIME;
            SousChef.Vector4 imp = new SousChef.Vector4(0.0f, 0.0f, -1.0f);
            Matrix4 rotate = Matrix4.MakeRotateYDeg(-this.Orientation) * Matrix4.MakeRotateXDeg(-this.Incline);
            imp = rotate * imp;

            this.Body.LinearVelocity = new Vector3(imp.X, imp.Y, imp.Z) * ServerPlayer.DASHSCALER;
            this.lastDashVelocity = this.Body.LinearVelocity;
#if PROJECT_DEBUG || PROJECT_WORLD_BUILDING
            this.dashCool = 0;     
#else
            this.dashCool = 5 * dashTicks;
#endif

            Game.SendSound(BBSound.lasercannonfire, Position);
        }

        /// <summary>
        /// Throw an object from the passed in hand
        /// </summary>
        /// <param name="hand"></param>
        public void Throw(string hand, float scalar)
        {
#if PROJECT_WORLD_BUILDING
            return;
#endif
            float orientation = this.Orientation;
            float incline = this.Incline;
            if (this.isStunned)
                return;
            
            if (this.Hands[hand].Held == null && this.LookingAt != null && 
                this.LookingAt.ObjectClass == GameObjectClass.Ingredient)
                this.PickUpObject((ServerIngredient)this.LookingAt);
            else
            {
                if (this.Hands[hand].Held == null)
                    return;

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

                // Throw object
                //this.Game.World.RemoveConstraint(spSlider1);
                this.Game.World.RemoveConstraint(constraint);
                if(held.GeomInfo.Size[2] >= held.GeomInfo.Size[0])
                    held.Position = moveOutsideBody(this.GeomInfo.Size[2] + held.GeomInfo.Size[2]); // put the object outside our body before throwing
                else
                    held.Position = moveOutsideBody(this.GeomInfo.Size[2] + held.GeomInfo.Size[0]); // put the object outside our body before throwing
                held.Body.LinearVelocity = new Vector3(imp.X, imp.Y, imp.Z) * ServerPlayer.THROWSCALER;
                held.Body.AngularVelocity = Vector3.Zero;
                _hand.Held = null;
                this.Game.SendSound(BBSound.punchmiss, Position);
                this.MarkDirty();
            }
        }

        /// <summary>
        /// Stuns the player from doing anything and dropping their item.
        /// </summary>
        public void Stun()
        {
            this.Throw("left", 0.0f); // Drop object in front of player.
            this.lastStunned = DateTime.Now.TimeOfDay;
            this.isStunned = true;
            Game.SendParticleEffect(BBParticleEffect.SPARKS, this.Position, 0, this.Id);
        }

        /// <summary>
        /// Checks if this player is stunned or not.
        /// </summary>
        /// <returns></returns>
        public bool IsStunned()
        {
            return this.isStunned;
        }

        private void PickUpObject(ServerIngredient obj)
        {
            if (this.isStunned)
                return;

            Game.SendSound(BBSound.bodyfall1, Position);
            constraint = new Generic6DofConstraint(
                this.Body, obj.Body,
                Matrix.Identity,// * Matrix.Translation(0, this.EyeHeight * 0.75f, 0),
                Matrix.Identity,// * Matrix.Translation(0, this.EyeHeight * 0.75f, HOLDDISTANCE), 
                true);
            constraint.AngularLowerLimit = new Vector3(0, 0, 0);
            constraint.AngularUpperLimit = new Vector3(0, 0, 0);
            constraint.LinearLowerLimit = new Vector3(0, 0, 0);
            constraint.LinearUpperLimit = new Vector3(0, 0, 0);

            this.Game.World.AddConstraint(constraint,true);
            this.Hands["left"] = new HandInventory(obj); // book keeping to keep track
            obj.LastPlayerHolding = this;
            MarkDirty();
        }

        /// <summary>
        /// Attempts to eject a cooker it is looking at.
        /// </summary>
        public void AttemptToEjectCooker()
        {
            if (this.isStunned)
                return;

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
            if (this.isStunned)
                return;

            if (this.canJump || Game.MultiJump)
            {
                this.Move(this.Body.LinearVelocity.X, ServerPlayer.JUMPSPEED, this.Body.LinearVelocity.Z);
                this.canJump = false;
                this.isFalling = false;
            }
        }

        /// <summary>
        /// We need to move the object out of our body for a few things. This will return an object 
        /// starts at our crosshairs outside out bodies.
        /// </summary>
        /// <returns>Position of the crosshair outside out body.</returns>
        private Vector3 moveOutsideBody(float howMuch)
        {
            Matrix4 rotMat = Matrix4.MakeRotateYDeg(-this.Orientation + 180);

            SCVector4 moveOutsideBody = new SCVector4(0, 0, 1);
            moveOutsideBody *= rotMat;
            moveOutsideBody *= howMuch;// this.GeomInfo.Size[2];

            // Check what the player is looking at
            Vector3 start = new Vector3
                (
                    this.Position.X + moveOutsideBody.X, //Math.Sin(this.Incline * Math.PI / 180.0f) *
                    this.Position.Y + moveOutsideBody.Y + this.EyeHeight,
                    this.Position.Z + moveOutsideBody.Z//this.GeomInfo.Size[0]) //Math.Sin(this.Incline * Math.PI / 180.0f) * 
               );
            return start;
        }

        /// <summary>
        /// Update method.
        /// </summary>
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
                    this.Position.Y + this.EyeHeight * 0.75f + (float)Math.Sin((this.Incline > 0 && this.canJump ? 0 : this.Incline) * Math.PI / 180.0f) * -HOLDDISTANCE, // TODO: 
                    this.Position.Z + (float)(Math.Cos(this.Orientation * Math.PI / 180.0f) * -HOLDDISTANCE));
            }

            Matrix4 rotMat = Matrix4.MakeRotateYDeg(-this.Orientation + 180);

            // Check what the player is looking at
            Vector3 start = moveOutsideBody(this.GeomInfo.Size[2]/2);

            SCVector4 yDir = new SCVector4
                (
                    0,
                    (float)Math.Sin(this.Incline * MathConstants.DEG2RAD) * -1,
                    (float)Math.Cos(this.Incline * MathConstants.DEG2RAD)
                );

            SCVector4 final = rotMat * yDir;
            final *= LINEOFSIGHTSCALAR;
            Vector3 end = new Vector3
                (
                    start.X + final.X,
                    start.Y + final.Y,
                    start.Z + final.Z
                );
            CollisionWorld.ClosestRayResultCallback raycastCallback = new CollisionWorld.ClosestRayResultCallback(start, end);
            this.Game.World.RayTest(start, end, raycastCallback);
            if (raycastCallback.HasHit)
            {
                //Filter Results
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

            if (DateTime.Now.TimeOfDay - this.lastStunned > STUNTIME)
                this.isStunned = true;

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
