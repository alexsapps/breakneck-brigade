using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SousChef;
using Tao.Ode;

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
        protected override GeometryInfo getGeomInfo() { return this.Game.Config.Cookers[Type.Name].GeomInfo; }

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
        public ServerCooker(CookerType type, ServerGame game, Ode.dVector3 transform)
            : base(game)
        {
            this.Type = type;
            this.Contents = new List<ServerIngredient>(); 
            AddToWorld(transform);
        }


        /// <summary>
        /// Write the intial creation packet for this object. Everything above the ===== is handled by the
        /// base class. Below that is object specific data and is handled in this function.
        /// 
        /// int32  id
        /// int16  objectclass
        /// bool   ToRender
        /// float  X
        /// float  Y
        /// float  Z
        /// ===========
        /// string type - the type of cooker i.e. pot, blender, the object is 
        /// </summary>
        /// <param name="stream"></param>
        public override void Serialize(BinaryWriter stream)
        {
            base.Serialize(stream);
            stream.Write(this.Type.Name);
            writeIngredients(stream);
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
                ingredient.ToRender = false; // hide the object
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
                Ode.dVector3 ingSpawn = new Ode.dVector3(this.Position.X + 30, this.Position.Y + 30, this.Position.Z + 30); // spawn above cooker for now TODO: Logically spawn depeding on cooker
                ServerIngredient ToAdd = new ServerIngredient(Type.Recipes[this.HashCache].FinalProduct, Game, ingSpawn);
                this.Contents.Add(ToAdd);
                return ToAdd;
            }
            return null;
        }

        /// <summary>
        /// Update the stream with the needed info. Everything above ==== is handled by the base class.
        /// Currently writes the entire conents of the cooker to the stream.  
        /// 
        /// int32  id
        /// bool   ToRender
        /// float  X
        /// float  Y
        /// float  Z
        /// ===========
        /// int16  count - the number o ingredients in the cooker. 
        /// *int32  id - the id of each ingredient in the cooker. There will be exactly count number of ids.  
        /// </summary>
        public override void UpdateStream(BinaryWriter stream)
        {
            base.UpdateStream(stream);
            writeIngredients(stream);
        }

        public override void Update()
        {
            base.Update();
        }

        protected void writeIngredients(BinaryWriter stream)
        {
            stream.Write((Int16)Contents.Count);
            foreach (var ingredient in Contents)
                stream.Write((Int32)ingredient.Id);
        }

        public override void OnCollide(ServerGameObject obj)
        {
            if (obj.ObjectClass == GameObjectClass.Ingredient)
            {
                this.AddIngredient((ServerIngredient)obj);
            }
        }

    }
}
