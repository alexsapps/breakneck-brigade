using SousChef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tao.OpenGl;

namespace Breakneck_Brigade
{
    class ClientPlayer : ClientGameObject
    {
        public override string ModelName { get { return "teapotPillar"; } }
        public ClientGameObject LookingAt { get; set; }

        public override float[] Sides { get { return BB.GetPlayerSides(); } }

        public ClientPlayer(int id, Vector4 position, ClientGame game)
            : base(id, game, position)
        {
            base.finalizeConstruction();
        }

        public ClientPlayer(int id, BinaryReader reader, ClientGame game)
            : base(id, game, reader)
        {
            int lookingAtId = reader.ReadInt32();
            this.LookingAt = lookingAtId != -1 ? this.Game.LiveGameObjects[lookingAtId] : null;

            base.finalizeConstruction();
        }

        public void Update(Vector4 position)
        {
            base.BaseUpdate(position);
        }

        public override void StreamUpdate(BinaryReader reader)
        {
            base.StreamUpdate(reader);
        }

        /// <summary>
        /// Renders the game object in the world.
        /// </summary>
        public override void Render()
        {
            if (this.ToRender && Game.PlayerObjId != this.Id)//&& this.Id != )
            {
                Gl.glPushMatrix();
                Gl.glMultMatrixf(Transformation.glArray);
                    Model.Render();
                Gl.glPopMatrix();
            }
        }

    }
}
