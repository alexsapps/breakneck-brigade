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
        IngredientType Type { get; set; }
        int Cleanliness { get; set; }

        public ServerIngredient (int id, IngredientType type, Server server)
            : base(id, new Vector4(), server)
        {
            this.Type = type;
            this.Cleanliness = 100;
        }

        public ServerIngredient(int id, IngredientType type, Vector4 transform, Server server)
            : base(id, transform, server)
        {
            this.Type = type;
            this.Cleanliness = 100;
        }

        void writeSerialization(StreamWriter stream)
        {

        }

        void Update()
        {
            //move the object
        }

    }
}
