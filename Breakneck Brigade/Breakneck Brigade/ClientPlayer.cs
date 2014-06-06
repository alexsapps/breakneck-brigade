using Breakneck_Brigade.Graphics;
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
        private string _modelName;
        public override string ModelName { get { return _modelName; } }
        public string TeamName { get; set; } // TODO: Make this control the texture used
        public ClientGameObject LookingAt { get; set; }
        public int LookingAtId { get; set; }
        public int HeldId { get; set; }

        public override float[] Sides { get { return BB.GetPlayerSides(); } }

        public static float eyeHeight = 0;

        public ClientPlayer(int id, Vector4 position, ClientGame game)
            : base(id, game, position)
        {
            base.finalizeConstruction();
        }

        public ClientPlayer(int id, BinaryReader reader, ClientGame game)
            : base(id, game, reader)
        {
            this.TeamName = reader.ReadString();
            LookingAtId = reader.ReadInt32();
            ClientGameObject tmp;
            this.Game.LiveGameObjects.TryGetValue(LookingAtId, out tmp);
            this.LookingAt = tmp;
            start = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            end = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            eyeHeight = reader.ReadSingle();

            //Add team indicator
            this._modelName = TeamName == "red" ? "redChef" : "chef";
            //game.ParticleSpawners.Add(new PSTeamIndicator(this));

            base.finalizeConstruction();
        }

        public void Update(Vector4 position)
        {
            base.BaseUpdate(position);
        }

        public override void StreamUpdate(BinaryReader reader)
        {
            base.StreamUpdate(reader);

            this.LookingAtId = reader.ReadInt32();
            start = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            end = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            eyeHeight = reader.ReadSingle();
            this.HeldId = reader.ReadInt32();
        }

        public static Vector4 start = new Vector4(), end = new Vector4();

        /// <summary>
        /// Renders the game object in the world.
        /// </summary>
        public override void Render()
        {
            if (this.ToRender && Game.PlayerObjId != this.Id)//&& this.Id != )
            {
                Gl.glPushMatrix();
                Gl.glMultMatrixf(this.Transformation.glArray);
                this.Model.Render();
                Gl.glPopMatrix();
            }
        }

        public override string ToString()
        {
            return TeamName + " Player";
        }

    }
}
