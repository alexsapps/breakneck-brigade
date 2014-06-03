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
        private const float EJECTSPEED = 5.0f;

        public override GameObjectClass ObjectClass { get { return GameObjectClass.Cooker; } }
        public Dictionary<int, ServerIngredient> Contents
        { get { return _contents; } private set { this.MarkDirty(); this._contents = value; } }
        public Dictionary<int, ServerIngredient> _contents;
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
            this._contents = new Dictionary<int, ServerIngredient>();
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
            stream.Write(this.Team.Name);
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
                this.recomputeTintList();
                this.Game.SendSound(BBSound.trayhit2, Position);
                this.MarkDirty();
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
            this.MarkDirty();
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
                if (this.CheckRecipe(recipe.Value))
                {
                    this.FinishCook(recipe.Value);
                    break;
                }
                   
            }
            
            return null;
        }

        private void FinishCook(Recipe recipe)
        {
            int cookScore = 0;
            List<ServerIngredient> toEject = new List<ServerIngredient>();
            // Final scan of the recipe to add up all the points
            foreach (var recIng in recipe.Ingredients)
            {
                List<ServerIngredient> matchingCont = new List<ServerIngredient>();
                matchingCont = this.ReturnContents(recIng, recIng.nCount + recIng.nOptional);
                if (matchingCont != null)
                {
                    toEject.AddRange(matchingCont);
                    cookScore += recIng.CalculateScore(matchingCont.Count);
                }
            }
            
            foreach(var ingredient in toEject) //toList because collection gets modified during enumeration
            {
                //remove all the ingredients from the game world
                ingredient.Remove();
            }

            Vector3 ingredientSpawningPoint = new Vector3(this.Position.X, this.Position.Y + this.GeomInfo.Size[1], this.Position.Z); // spawn above cooker for now TODO: Logically spawn depeding on cooker
            ServerIngredient newIngredient = new ServerIngredient(recipe.FinalProduct, this.Game, ingredientSpawningPoint);
            newIngredient.Body.ApplyImpulse(new Vector3(0, EJECTSPEED, 0), ingredientSpawningPoint);
            this.Game.SendParticleEffect(BBParticleEffect.SMOKE, this.Position, (int)SmokeType.GREY);
            this.Game.SendSound(BBSound.trayhit1, this.Position);
            this.MarkDirty();
        }


        public bool CheckRecipe(Recipe recipe)
        {
            foreach (var recIng in recipe.Ingredients)
            {
                if (!CheckContents(recIng.Ingredient, recIng.nCount))
                    return false;
            }
            return true;
        }

        public bool CheckContents(IngredientType ingType, int numEssential)
        {
            int found = 0;
            foreach (var content in this.Contents.Values)
            {
                if (ingType == content.Type)
                    found++;
            }
            return found >= numEssential;
        }

        public List<ServerIngredient> ReturnContents(RecipeIngredient ing, int needed)
        {
            List<ServerIngredient> matchingIng = new List<ServerIngredient>();
            foreach (var content in this.Contents.Values)
            {
                if (ing.Ingredient == content.Type)
                {
                    matchingIng.Add(content);
                    if (matchingIng.Count >= needed)
                    {
                        break;
                    }
                }
            }

            return matchingIng;
        }


        /// <summary>
        /// Ejects ingredients from this cooker.
        /// </summary>
        public void Eject()
        {
            foreach (ServerIngredient containedIngredient in this.Contents.Values.ToList())
            {
                Vector3 ingredientSpawningPoint = new Vector3(this.Position.X, this.Position.Y + this.GeomInfo.Size[1], this.Position.Z); // spawn above cooker for now TODO: Logically spawn depeding on cooker
                ServerIngredient newIngredient = new ServerIngredient(containedIngredient.Type, this.Game, ingredientSpawningPoint);
                newIngredient.Body.ApplyImpulse(new Vector3(0, EJECTSPEED, 0), ingredientSpawningPoint);
            }
            this.Contents.Clear();
            this.Game.SendSound(BBSound.boom1, Position);
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
                ((ServerIngredient)obj).LastPlayerHolding != null &&
                ((ServerIngredient)obj).LastPlayerHolding.Client.Team == Team)
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
                foreach (var potentialRec in this.Type.RecipeHash[ing.Type.Name]) 
                {
                    foreach (var potentialIng in potentialRec.Ingredients)
                    {
                        bool skip = false;
                        foreach(var ing2 in Contents.Values)
                        {
                            if (ing2.Type == potentialIng.Ingredient)
                            {
                                skip = true;
                                break; //don't tint if already in contents
                            }
                        }
                        if (!skip)
                        {
                            this.Team.TintList.Add(potentialIng.Ingredient.Name);
                        }
                    }
                }
            }
            // add to the list of server events
            Game.SendTintListUpdate(this.Team);
        }

    }
}
