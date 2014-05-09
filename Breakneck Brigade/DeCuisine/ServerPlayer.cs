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
        protected override GeometryInfo getGeomInfo() { return new GeometryInfo() { Mass = 5, Shape = GeomShape.Box, Sides = new float[] { 1.0f, 3.0f, 1.0f } }; }

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
        public void Move(float x, float y, float z)
        {
            Vector3 newPos = new Vector3(Position.X + x, Position.Y + y, Position.Z + z);
            this.Position = newPos;
        }

        public override void OnCollide(ServerGameObject obj)
        {
            //if (obj.ObjectClass == GameObjectClass.Ingredient)
            //{
            //    this.toHold = obj;
            //}

        }


        private void makeJoint(string hand, ServerGameObject obj)
        {
            /*
            HingeJoint joint = this.Game.World.CreateHingeJoint();
            joint.Attach(this.Body, obj.Body);
            OdeDotNet.Vector3 pos = this.Geom.Position; // Ode.dGeomGetPosition(this.Geom);
            obj.Body.Position = new OdeDotNet.Vector3(pos.X, pos.Y - 10, pos.Z + 10);
            joint.Anchor = new OdeDotNet.Vector3(pos[0] - 10, pos[1] - 10, pos[2] + 10);
            joint.Anchor2 = new OdeDotNet.Vector3(pos[0] - 10, pos[1] - 10, pos[2] + 10);
            joint.Axis = new OdeDotNet.Vector3(0, 0, 1.0f);
            */
            /*
            Ode.dVector3 pos = Ode.dGeomGetPosition(this.Geom);
            Ode.dJointAttach(joint, this.Body, obj.Body);
            Ode.dBodySetPosition(obj.Body, pos.X  , pos.Y - 10, pos.Z + 10);
            Ode.dJointSetHingeAnchor(joint, pos[0] - 10, pos[1] - 10 , pos[2] + 10);
            Ode.dJointSetHingeAxis(joint, 0, 0, 1.0);
            */
            
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
            //// Can't manipulate Ode in collide funtions, so a hacked flag has to do to make the joint
            //if (this.toHold != null)
            //{
            //    this.makeJoint("left", this.toHold);
            //    this.toHold = null;
            //}
        }
    }
}
