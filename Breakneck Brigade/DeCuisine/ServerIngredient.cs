using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using System.IO;
using Tao.Ode;

namespace DeCuisine
{
    class ServerIngredient : ServerGameObject, IIngredient 
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Ingredient; } }
        public IngredientType Type { get; set; }
        public int Cleanliness { get; set; }

        public ServerIngredient(int id, IngredientType type, ServerGame game)
            : base(id, game)
        {
            this.Type = type;
        }

        /// <summary>
        /// Create the object and add it to the game world at vector4. Also added it to the 
        /// serial stream passed in.
        /// </summary>
        public ServerIngredient(int id, IngredientType type, ServerGame game, Vector4 transform, BinaryWriter stream) 
            : base(id, game)
        {
            this.Type = type;
            this.AddToWorld(transform[0], transform[1], transform[2]);
            this.Serialize(stream); // add it to the stream to be created
        }

        public override void Serialize(BinaryWriter stream)
        {
            base.Serialize(stream);
            this.UpdateStream(stream);
            stream.Write(Type.Name);
            stream.Write((int)Cleanliness);
        }


        /// <summary>
        /// Update the stream with the needed info. Currently just the positions so 
        /// it is handled in the base class
        /// </summary>
        /// <param name="stream"></param>
        public override void UpdateStream(BinaryWriter stream)
        {
            base.UpdateStream(stream);
        }

        public override void Update()
        {
            
        }

        public override GeometryInfo GeomInfo
        {
            get { return this.Game.Config.Ingredients[Type.Name].GeomInfo; }
        }
    }
}
