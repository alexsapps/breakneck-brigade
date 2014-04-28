using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breakneck_Brigade.Graphics;
using SousChef;
using System.IO;

namespace Breakneck_Brigade
{
    class ClientIngredient : ClientGameObject, IGameObject
    {
        public IngredientType Type { get; set; }
        public int Cleanliness { get; set; }

        public override string ModelName { get { return Type.Name; } }

        /// <summary>
        /// Creates an Ingredient of the type passed in at the center of the game world. Used for 
        /// objects not created on the server
        /// </summary>
        /// <param name="id">Unique object id</param>
        /// <param name="type">What type the ingrident is i.e. "tomato"</param>
        /// <param name="transform">The initial location you want to place the ingredient</param>
        /// <param name="client">The client making this object</param>
        /// <param name="model">The ingame model of the ingredient</param>
        public ClientIngredient(int id, IngredientType type, Vector4 transform, int cleanliness, ClientGame game)
            : base(id, game)
        {
            this.Type = type;
            this.Cleanliness = cleanliness;
            base.finilizeConstruction();
        }

        /// <summary>
        /// called by ClientGameObject.Deserialize. Used by objects that were created on the server
        /// The packet for reader will look as follows. Everything above ==== is read by the 
        /// base constructor. The class specific constructor just needs to read everything after.
        /// float X
        /// float Y
        /// float Z
        /// ===========
        /// string name
        /// int32 cleanliness
        /// </summary>
        public ClientIngredient(int id, BinaryReader reader, ClientGame game)
            : base(id, game, reader)
        {

            string ingrname = reader.ReadString();
            this.Type = game.Config.Ingredients[ingrname];
            this.Cleanliness = reader.ReadInt32();
            base.finilizeConstruction(); //set the model based on the type of object
        }


        /// <summary>
        /// Update everything pertaining to the ingriedient. The position 
        /// is handled by the super class.
        /// </summary>
        private void update(Vector4 transform, int cleanliness)
        {
            base.Update(transform);
            this.Cleanliness = cleanliness;
        }

        /// <summary>
        /// Read the stream and update values. The packet will look as follows. 
        /// Everything under ==== is handled by the base class.
        /// 
        /// double x
        /// double y
        /// double z
        /// ==========
        /// int16  cleanliness
        /// </summary>
        public override void StreamUpdate(BinaryReader reader)
        {
            base.StreamUpdate(reader); //updates the position of the object as well
            this.Cleanliness = reader.ReadInt16(); 
        }

        /// <summary>
        /// Display the ingridient in the game world.
        /// </summary>
        public override void Render()
        {
            base.Render();
        }

    }
}
