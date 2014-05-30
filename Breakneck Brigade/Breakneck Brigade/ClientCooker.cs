using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Breakneck_Brigade.Graphics;
using System.IO;

namespace Breakneck_Brigade
{
    class ClientCooker : ClientGameObject
    {
        public CookerType Type { get; set; }
        public List<ClientIngredient> Contents { get; set; }

        public override float[] Sides { get { return Type.GeomInfo.Size; } }
        public override string ModelName { get { return Type.Name; } }

        /// <summary>
        /// called by ClientGameObject.Deserialize. Used by objects that were created on the server
        /// The packet for reader will look as follows. Everything above ==== is read by the 
        /// base constructor. The class specific constructor just needs to read everything after.
        /// float X
        /// float Y
        /// float Z
        /// ===========
        /// string name - the name of the type of cooker. 
        /// </summary>
        public ClientCooker(int id, BinaryReader reader, ClientGame game)
            : base(id, game, reader)
        {
            string cookerType = reader.ReadString();
            this.Type = game.Config.Cookers[cookerType];
            this.Contents = new List<ClientIngredient>();
            processIngredients(reader);
            base.finalizeConstruction(); //set the model based on the type of object
        }

        /// <summary>
        /// Read from the stream. Everything above ==== is handled by the base class.
        /// should read every ingredient that was added to the cooker. 
        /// 
        /// int32  id
        /// float  X
        /// float  Y
        /// float  Z
        /// ===========
        /// int16  count - the number o ingredients in the cooker. 
        /// *int32  id - the id of each ingredient in the cooker. There will be exactly count number of ids.  
        /// </summary>
        public override void StreamUpdate(BinaryReader reader)
        {
            base.StreamUpdate(reader);
            this.processIngredients(reader);
        }

        /// <summary>
        /// helps process ingredients added
        /// </summary>
        private void processIngredients(BinaryReader reader)
        {
            int count = reader.ReadInt16();
            Contents.Clear();
            for (int i = 0; i < count; i++)
            {
                int x = reader.ReadInt32();
                Contents.Add((ClientIngredient)Game.LiveGameObjects[x]);
            }
        }

        public override void Render()
        {
            if (this.Game.LookatId == this.Id)
                base.RenderColored();
            else
                base.Render();
        }
    }


}
