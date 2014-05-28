using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using SousChef;

using BulletSharp;
using System.Diagnostics;

namespace DeCuisine
{
    /// <summary>
    /// The server cooker object
    /// </summary>
    class ServerCooker : ServerGameObject
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Cooker; } }
        public Dictionary<int, ServerIngredient> Contents { get; private set; }
        public CookerType Type { get; set; }
        protected override GeometryInfo getGeomInfo() { return this.Game.Config.Cookers[Type.Name].GeomInfo; }
        public ServerTeam Team { get; set; }
        

        private string HashCache { get; set; }
        private int ParticleEffect { get; set; }

        public override int SortOrder { get { return 5000; } } /* must be sent after ingredients, because cookers contain ingredients */

        /// <summary>
        /// Makes a servercooker object on the server
        /// </summary>
        /// <param name="id">Unique id on the server. Should be given to the client to make
        /// a ClientCooker with the same id</param>
        /// <param name="transform">Initial location</param>
        /// <param name="server">The server where the cooker is made</param>
        /// <param name="type">What type of cooker i.e "oven"</param>
        public ServerCooker(CookerType type, ServerTeam team, ServerGame game, Vector3 transform)
            : base(game)
        {
            this.Type = type;
            this.Contents = new Dictionary<int, ServerIngredient>();
            this.Team = team;
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
            if (Type.ValidIngredients.Contains(ingredient.Type.Name))
            {
                HashCache = null;
                Contents.Add(ingredient.Id, ingredient);
                ingredient.ToRender = false; // hide the object
                ingredient.Removed += ingredient_Removed;
                this.Game.Controller.ScoreAdd(ingredient.LastPlayerHolding, ingredient);
                this.recomputeTintList(); 
                return true;
            }
            return false;
        }

        // happens when an ingredient is removed from the world
        private void ingredient_Removed(object sender, EventArgs e)
        {
            var ingredient = ((ServerIngredient)sender);
            Contents.Remove(ingredient.Id);
            ingredient.Removed -= ingredient_Removed;
        }

        /*
         * Should loop over the contents, calculate the score, attatch the score to the 
         * return object and return a final product with the attatched score. 
         * TODO: Make it do that^
         */
        public ServerIngredient Cook()
        {
            var copyContents = this.Contents;
            //find if there is a valid recipe
            foreach (var recipe in this.Type.Recipes)
            {
                if (CheckRecipe(recipe.Value))
                    finishCook(recipe.Value);
            }
            
            return null;
        }
        private void finishCook(Recipe recipe)
        {
            int cookScore = 0;
            List<ServerIngredient> toEject = new List<ServerIngredient>();
            // Final scan of the recipe to add up all the points
            foreach (var recIng in recipe.Ingredients)
            {
                var content = ReturnContents(recIng.Ingredient);
                if (content != null)
                {
                    toEject.Add(content);
                    cookScore += recIng.Ingredient.DefaultPoints;
                }
            }
            
            foreach(var ingredient in toEject) //toList because collection gets modified during enumeration
            {
                //remove all the ingredients from the game world
                ingredient.Remove();
            }
            var ingSpawn = new Vector3(this.Position.X, this.Position.Y + 100, this.Position.Z); // spawn above cooker for now TODO: Logically spawn depeding on cooker
            var newIng = new ServerIngredient(recipe.FinalProduct, Game, ingSpawn);
            newIng.Body.LinearVelocity = new Vector3(0, 500, 0);
        }


        public bool CheckRecipe(Recipe recipe)
        {
            foreach (var recIng in recipe.Ingredients)
            {
                if (recIng.nOptional > 0) //todo: wrong
                    continue; // pass over optional ingredients
                if (!CheckContents(recIng.Ingredient))
                    return false;
            }
            return true;
        }

        public bool CheckContents(IngredientType ingType)
        {
            foreach (var content in this.Contents.Values)
            {
                if (ingType == content.Type)
                    return true;
            }
            return false;
        }

        public ServerIngredient ReturnContents(IngredientType ingType)
        {
            foreach (var content in this.Contents.Values)
            {
                if (ingType == content.Type)
                    return content;
            }
            return null;
        }


        /// <summary>
        /// Ejects ingredients from this cooker.
        /// </summary>
        public void Eject()
        {
            foreach (ServerIngredient containedIngredient in this.Contents.Values.ToList())
            {
                Vector3 ingredientSpawningPoint = new Vector3(this.Position.X, this.Position.Y + 200, this.Position.Z); // spawn above cooker for now TODO: Logically spawn depeding on cooker
                ServerIngredient newIng = new ServerIngredient(containedIngredient.Type, this.Game, ingredientSpawningPoint);
            }
            this.Contents.Clear();
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

        protected override void updateHook()
        {
           
        }

        protected void writeIngredients(BinaryWriter stream)
        {
            stream.Write((Int16)Contents.Count);
            foreach (var ingredient in Contents.Values)
                stream.Write((Int32)ingredient.Id);
        }

        public override void OnCollide(ServerGameObject obj)
        {
            base.OnCollide(obj);
            if (obj.ObjectClass == GameObjectClass.Ingredient && 
                !this.Contents.ContainsKey(obj.Id) && 
                ((ServerIngredient)obj).LastPlayerHolding != null)
            {
                this.AddIngredient((ServerIngredient)obj);
            }
        }

        /// <summary>
        /// recompute the tint list and add it to the teams tint list
        /// </summary>
        private void recomputeTintList()
        {
            foreach (var ing in this.Contents.Values)
            {
                foreach (var potentialRec in this.Type.RecipeHash[ing.Type.Name].ToList()) 
                {
                    foreach(var potentialIng in potentialRec.Ingredients)
                        this.Team.TintList.Add(potentialIng.Ingredient.Name);
                }
            }
            // add to the list of server events
            this.Game.ServerEvents.Add(new ServerSendTintList(){Team = this.Team.Name, TintList = this.Team.TintList.ToList()});
        }

    }
}
