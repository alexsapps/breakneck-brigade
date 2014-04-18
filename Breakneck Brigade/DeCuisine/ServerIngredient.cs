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
        public IngredientType Type { get; set; }
        public int Cleanliness { get; set; }

        public ServerIngredient(int id, IngredientType type, Vector4 transform, ServerGame game)
            : base(id, transform, game)
        {
            this.Type = type;
        }

        void writeSerialization(StreamWriter stream)
        {

        }

        public override void Update()
        {
            //move the object
        }

    }
}
