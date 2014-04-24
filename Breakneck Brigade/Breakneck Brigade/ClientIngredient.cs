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
            : base(id, transform, game)
        {
            construct(type, cleanliness);
        }

        //called by ClientGameObject.Deserialize. Used by objects that were created on the server
        public ClientIngredient(int id, BinaryReader reader, ClientGame game) 
            : base(id, reader, game)
        {
            //todo: get type from string (config file): Type = f(reader.ReadString());
            IngredientType type = null;
            var cleanliness = reader.ReadInt32();
            construct(type, cleanliness);
        }

        private void construct(IngredientType type, int cleanliness)
        {
            this.Type = type;
            update(cleanliness);
        } 


        /// <summary>
        /// Update everything pertaining to the ingriedient. The position 
        /// is handled by the super class.
        /// </summary>
        private void update(int cleanliness)
        {
            this.Cleanliness = cleanliness;
        }

        /// <summary>
        /// Update everything pertaining to the ingriedient. The position 
        /// is handled by the super class.
        /// </summary>
        private void update(Vector4 transform, int cleanliness)
        {
            base.Update(transform);
            this.update(cleanliness);
        }

        public override void StreamUpdate(BinaryReader reader)
        {
            base.StreamUpdate(reader); //updates the position of the object as well
            int cleanliness = reader.ReadInt32(); 
            update(cleanliness);
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
