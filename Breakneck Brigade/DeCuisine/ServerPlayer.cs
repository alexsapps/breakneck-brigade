using SousChef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.Ode;

namespace DeCuisine
{
    class ServerPlayer : ServerGameObject
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Player; } }
        protected override GeometryInfo getGeomInfo() { return new GeometryInfo() { Mass = 5, Shape = GeomShape.Box, Sides = new float[] { 1.0f, 3.0f, 1.0f } }; }

        public ServerPlayer(ServerGame game, Ode.dVector3 position) 
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
            Ode.dVector3 newPos = new Ode.dVector3(Position.X + x, Position.Y + y, Position.Z + z);
            this.Position = newPos;
        }
    }
}
