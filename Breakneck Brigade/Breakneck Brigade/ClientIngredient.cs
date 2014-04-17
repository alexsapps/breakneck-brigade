using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breakneck_Brigade.Graphics;
using SousChef;

namespace Breakneck_Brigade
{
    class ClientIngredient : ClientGameObject, IGameObject
    {
        public IngredientType Type { get; set; }
        public int Cleanliness { get; set; }
        
        /// <summary>
        /// Creates an Ingredient at the center of the game world
        /// </summary>
        /// <param name="id">Unique object id</param>
        /// <param name="type">What type the ingrident is i.e. "tomato"</param>
        /// <param name="client">The client making this object</param>
        /// <param name="model">The ingame model of the ingredient</param>
        public ClientIngredient(int id, IngredientType type, Client client, Model model) 
            : base(id, new Vector4(), client, model)
        {
            this.Type = type;
            this.Cleanliness = 100;
        }

        /// <summary>
        /// Creates an Ingredient of the type passed in at the center of the game world
        /// </summary>
        /// <param name="id">Unique object id</param>
        /// <param name="type">What type the ingrident is i.e. "tomato"</param>
        /// <param name="transform">The initial location you want to place the ingredient</param>
        /// <param name="client">The client making this object</param>
        /// <param name="model">The ingame model of the ingredient</param>
        public ClientIngredient(int id, IngredientType type, Vector4 transform, Client client, Model model)
            : base(id, transform, client, model)
        {
            this.Type = type;
            this.Cleanliness = 100;
        }

        /// <summary>
        /// Display the ingridient in the game world.
        /// </summary>
        public override void Render()
        {
            // render the object
        }

    }
}
