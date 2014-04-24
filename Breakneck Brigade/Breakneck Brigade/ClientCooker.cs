using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using Breakneck_Brigade.Graphics;
using System.IO;
using SousChef;

namespace Breakneck_Brigade
{
    class ClientCooker : ClientGameObject
    {
        public CookerType Type { get; set; }
        public List<ClientIngredient> Contents { get; set; }
        public ClientCooker(int id, CookerType type, Vector4 transform, ClientGame game)
            : base(id, new Vector4(), game)
        {
            construct(type);
        }

        //called by ClientGameObject.Deserialize
        public ClientCooker(int id, BinaryReader reader, ClientGame game) 
            : base(id, reader, game)
        {
            //CookerType type = reader.ReadString();
            string name = reader.ReadString(); //TODO: will need to read the name and then
                                               //assign it the correct recipe using the config file
            construct(null); //TODO: make it use the dynamic cooker type from above
        }

        private void construct(CookerType type)
        {
            this.Type = type;
        }

        private void update()
        {
            // add any client specific update here
        }

        /// <summary>
        /// Update everything pertaining to the ingriedient. Note this is 
        /// mainly for ease of testing, shouldn't be called by the stream
        /// </summary>
        private void update(Vector4 transform, int cleanliness)
        {
            base.Update(transform);
            this.update();
        }


        public override void StreamUpdate(BinaryReader reader)
        {
            base.StreamUpdate(reader);
            if (reader.ReadBoolean()) // Ingredient added
            {
                processIngredientsAdded(reader);
            }
            this.update();
        }

        private void processIngredientsAdded(BinaryReader reader)
        {
            for (int x = 0; x < reader.ReadInt16(); x++)
            {
                //TODO: Find the ingredient with the ID and put it in the contents of 
                //this cooker. 
                int id = reader.ReadInt16();

                //TODO: Find the effect to play and then play it. 
                int effect = reader.ReadInt16();
            }
        }

        public override void Render()
        {
            base.Render();
        }
    }

}
