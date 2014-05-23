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

        public string Name { get; set; }

        private bool isFalling { get; set; }
        private bool canJump { get; set; }
        private Vector3 lastVelocity { get; set; }

        protected override GeometryInfo getGeomInfo() { return BB.GetPlayerGeomInfo(); }

        /// <summary>
        /// Gets the ServerObject this player is currently looking at.
        /// </summary>
        public ServerGameObject ObjectBeingLookedAt { get; private set; }
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
        }

        public override void Serialize(BinaryWriter stream)
        {
            base.Serialize(stream);
            stream.Write(this.Client.Team.Name);
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
                this.Hands["left"] = new HandInventory(obj); // book keeping to keep track
                ((ServerIngredient)obj).LastPlayerHolding = this;
            }

        }


        public void Dash()
        {
            SousChef.Vector4 imp = new SousChef.Vector4(0.0f, 0.0f, -1.0f);
            Matrix4 rotate = Matrix4.MakeRotateYDeg(-this.Orientation) * Matrix4.MakeRotateXDeg(-this.Incline);
            imp = rotate * imp;

            this.Body.LinearVelocity = new Vector3(imp.X, imp.Y, imp.Z) * ServerPlayer.DASHSCALER;
        }

        /// <summary>
        /// Throw an object from the passed in hand
        /// </summary>
        /// <param name="hand"></param>
        public void Throw(string hand, float orientation, float incline)
        {
            SousChef.Vector4 imp = new SousChef.Vector4(0.0f, 0.0f, -1.0f);
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
            this.Hands[hand].Held.Body.LinearVelocity = new Vector3(imp.X, imp.Y, imp.Z) * ServerPlayer.THROWSCALER;
            this.Hands[hand] = new HandInventory(null); //clear the hands
        }

        /// <summary>
        /// Attempts to eject a cooker it is looking at.
        /// </summary>
        public void AttemptToEjectCooker()
        {
            if(this.ObjectBeingLookedAt != null && this.ObjectBeingLookedAt.ObjectClass == GameObjectClass.Cooker)
            {
                ((ServerCooker)this.ObjectBeingLookedAt).Eject();
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

        /// <summary>
        /// Queries for UI purposes.
        /// TODO: Probably need to move all these queries to their respective objects ToString().
        /// </summary>
        /// <returns>Information about what this player is looking at.</returns>
        /*
        public string Query()
        {
            string result = string.Empty;
            if (this.ObjectBeingLookedAt != null)
            {
                if (this.ObjectBeingLookedAt.ObjectClass == GameObjectClass.Cooker)
                {
                    ServerCooker cookerLookedAt = (ServerCooker)this.ObjectBeingLookedAt;
                    result += cookerLookedAt.Type.Name + " which currently has: ";
                    foreach (ServerIngredient ingredient in cookerLookedAt.Contents.Values.ToList())
                    {
                        result += ingredient.Type.Name + " ";
                    }
                }
                else if (this.ObjectBeingLookedAt.ObjectClass == GameObjectClass.Ingredient)
                {
                    ServerIngredient ingredientLookedAt = (ServerIngredient)this.ObjectBeingLookedAt;
                    result += ingredientLookedAt.Type.Name;
                }
            }

            return result;
        }
         */

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
                                                                                                    this.Position.Y + (float)Math.Sin(this.Incline * Math.PI / 180.0f) * -HOLDDISTANCE, 
                                                                                                    this.Position.Z + (float)Math.Cos(this.Orientation * Math.PI / 180.0f) * -HOLDDISTANCE);
            }

            // Check what the player is looking at
            // Get player position and angle and then where it should end
            /*
            Vector3 start = new Vector3
                (
                    this.Position.X + (float)(this.GeomInfo.Sides[0] * Math.Sqrt(2) * Math.Sin(this.Incline * Math.PI / 180.0f) * Math.Sin(this.Orientation * Math.PI / 180.0f)),
                    this.Position.Y + (float)(this.GeomInfo.Sides[0] * Math.Sqrt(2) * Math.Cos(this.Incline * Math.PI / 180.0f)),
                    this.Position.Z + (float)(this.GeomInfo.Sides[0] * Math.Sqrt(2) * Math.Sin(this.Incline * Math.PI / 180.0f) * Math.Cos(this.Orientation * Math.PI / 180.0f))
                );

            Vector3 end = new Vector3
                (
                    start.X + (float)(LINEOFSIGHTSCALAR * Math.Sin(this.Incline * Math.PI / 180.0f) * Math.Sin(this.Orientation * Math.PI / 180.0f)),
                    start.Y + (float)(LINEOFSIGHTSCALAR * Math.Cos(this.Incline * Math.PI / 180.0f)),
                    start.Z + (float)(LINEOFSIGHTSCALAR * Math.Sin(this.Incline * Math.PI / 180.0f) * Math.Cos(this.Orientation * Math.PI / 180.0f) * -1)
                );
             */
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
                ServerGameObject collidedGameObject = (ServerGameObject)collisionCallback.CollisionObject.CollisionShape.UserObject;
            }
            else
            {
                this.ObjectBeingLookedAt = null;
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
