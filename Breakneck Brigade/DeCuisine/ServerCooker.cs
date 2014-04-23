using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SousChef;

namespace DeCuisine
{
    /// <summary>
    /// The server cooker object
    /// </summary>
    class ServerCooker : ServerGameObject
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Cooker; } }
        public List<ServerIngredient> Contents { get; private set; }
        public CookerType Type { get; set; }
        public List<ServerIngredient> ToAdd { get; set; }

        private string HashCache { get; set; }
        private int ParticleEffect { get; set; }

        /// <summary>
        /// Makes a servercooker object on the server
        /// </summary>
        /// <param name="id">Unique id on the server. Should be given to the client to make
        /// a ClientCooker with the same id</param>
        /// <param name="transform">Initial location</param>
        /// <param name="server">The server where the cooker is made</param>
        /// <param name="type">What type of cooker i.e "oven"</param>
        public ServerCooker(int id, CookerType type, ServerGame game)
            : base(game)
        {
            this.Type = type;
            this.Contents = new List<ServerIngredient>();
            this.ToAdd = new List<ServerIngredient>();
        }

        public override void Serialize(BinaryWriter stream)
        {
            base.Serialize(stream);
            base.UpdateStream(stream); // puts position in the stream to send. Note, only need the base class updatestream for construction
            stream.Write(Type.Name); // tell which type of cooker to make on the client
        }


        /*
         * Adds the ingredient to the list. Keeps the list in sorted order. If the 
         * ingredient to add isn't in the valid ingredient table, don't add and return false
         */
        public bool AddIngredient(ServerIngredient ingredient)
        {
            HashCache = null;
            if (Type.ValidIngredients.Contains(ingredient.Type.Name))
            {
                Contents.Add(ingredient);
                this.Game.ObjectChanged(ingredient);
                this.Cook(); // check if you can cook. 
                return true;
            }
            return false;
        }

        /*
         * Should loop over the contents, calculate the score, attatch the score to the 
         * return object and return a final product with the attatched score. 
         * TODO: Make it do that^
         */
        public ServerIngredient Cook()
        {
            if (HashCache == null)
            {
                //recompute hash since an item was added since last cook
                Contents = Contents.OrderBy(o => o.Type.Name).ToList();//put in sorted order before hashing
                this.HashCache = Recipe.Hash(Contents.ConvertAll<IngredientType>(x => x.Type));
            }

            if (Type.Recipes.ContainsKey(this.HashCache))
            {
                foreach(var ingredeint in this.Contents)
                {
                    //remove all the ingredients from the game world
                    ingredeint.MarkDeleted();
                }
                this.Contents = new List<ServerIngredient>(); // clear contents
                ServerIngredient ToAdd = new ServerIngredient(Game.getId(), Type.Recipes[this.HashCache].FinalProduct, Game);
                this.Contents.Add(ToAdd);
                return ToAdd;
            }
            return null;
        }

        public override void UpdateStream(BinaryWriter stream)
        {
            base.UpdateStream(stream);
            // Calvin TODO: check collision between this cooker and ingredients that are touching it. 
            // if their is a collision, populate the list ToAdd with the ingredients. 
            packageIngredientsAdded(stream);
        }

        private void packageIngredientsAdded(BinaryWriter stream)
        {
            foreach (var ingredient in this.Contents)
            {
                stream.Write((Int16)ingredient.Id);
            }
        }

        public override void Update()
        {
            throw new NotImplementedException();
        }


        public override void OnCollide(ServerGameObject obj)
        {
            if (obj.ObjectClass == GameObjectClass.Ingredient)
            {
                this.AddIngredient((ServerIngredient)obj);
            }
        }
        public override GeometryInfo GeomInfo
        {
            get { return this.Game.Config.Cookers[Type.Name].GeomInfo; }
        }
    }
}
