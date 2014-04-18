using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using System.IO;

namespace DeCuisine
{
    class ServerIngredient : ServerGameObject, IIngredient 
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Ingredient; } }
        public IngredientType Type { get; set; }
        public int Cleanliness { get; set; }

        public ServerIngredient(int id, IngredientType type, Vector4 transform, ServerGame game)
            : base(id, transform, game)
        {
            this.Type = type;
        }

        public override void Serialize(BinaryWriter stream)
        {
            base.Serialize(stream);
            stream.Write(Type.Name);
            stream.Write((int)Cleanliness);
        }

        public override void Update()
        {
            //move the object
        }

    }
}
