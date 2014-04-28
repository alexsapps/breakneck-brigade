using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SousChef;
using Tao.Ode;

namespace DeCuisine
{
    class ServerBox : ServerGameObject
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Box; } }
        public Ode.dVector3 c1 { get; set; }
        public Ode.dVector3 c2 { get; set; }
        public string Texture { get; set; }
        public override bool HasBody { get { return false; } }
        protected override GeometryInfo getGeomInfo() { throw new NotSupportedException(); }
        public override Ode.dVector3 Position { get { throw new NotSupportedException("use c1 and c2 instead"); } set { throw new NotSupportedException("use c1 and c2 instead"); } }

        public ServerBox(ServerGame game, string texture, Ode.dVector3 c1, Ode.dVector3 c2) : base(game)
        {
            this.Texture = texture;
            this.c1 = c1;
            this.c2 = c2;

            AddToWorld(() =>
            {
                return Ode.dCreateBox(game.Space, c1.X, c1.Y, c1.Z); //do this instead of AddToWorld
            });
            if (DateTime.Now > new DateTime(2014, 04, 26))
                throw new Exception("hey calvin--dCreateBox only takes 3 floats?  we have 6 to process.");
        }
        public override void Update()
        {

        }
        public override void Serialize(System.IO.BinaryWriter stream)
        {
            base.serializeEssential(stream);
            stream.Write(c1);
            stream.Write(c2);
            stream.Write(Texture);
        }
        public override void UpdateStream(System.IO.BinaryWriter stream)
        {
            throw new NotSupportedException();
        }
    }
}
