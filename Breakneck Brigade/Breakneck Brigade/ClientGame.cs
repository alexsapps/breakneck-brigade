using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Breakneck_Brigade.Graphics;

namespace Breakneck_Brigade
{
    class ClientGame
    {
        public BBLock Lock;

        public int? PlayerObjId { get; set; }

        public TimeSpan GameTime { get; set; }

        //locking directly on gameObjects before accessing
        public Dictionary<int, ClientGameObject> LiveGameObjects { get; set; }
        public Dictionary<int, ClientGameObject> GameObjectsCache { get; set; }
        public Dictionary<string, HashSet<string>> TintedObjects { get; protected set; }
        public List<IngredientType> Goals { get; protected set; }
        public int LookatId { get; set; }
        public int HeldId { get; set; }
        public List<AParticleSpawner> ParticleSpawners { get; set;}

        public ConfigSalad Config { get; private set; }
        public List<Recipe> RequiredRecipies { get; private set; }

        public ClientGame(BBLock @lock)
        {
            Lock = @lock;
            LiveGameObjects = new Dictionary<int, ClientGameObject>();
            GameObjectsCache = new Dictionary<int, ClientGameObject>();
            ParticleSpawners = new List<AParticleSpawner>();
            Goals = new List<IngredientType>();
            Config = new GameObjectConfig().GetConfigSalad();
            this.CalculateRequiredRecipes();
            TintedObjects = new Dictionary<string, HashSet<string>>();
            TintedObjects.Add("red", new HashSet<string>());
            TintedObjects.Add("blue", new HashSet<string>());
            LookatId = -1;
            HeldId = -1;
        }

        /// <summary>
        /// Returns the list of recipies needed to complete the goals
        /// </summary>
        /// <returns></returns>
        public void CalculateRequiredRecipes()
        {
            Dictionary<string, Recipe> masterList = new Dictionary<string, Recipe>();
            List<Recipe> newRecipes = new List<Recipe>(), oldRecipes = new List<Recipe>();

            foreach (IngredientType ingriendient in this.Goals)
            {
                Recipe recipe;
                if (this.Config.Recipes.TryGetValue(ingriendient.Name, out recipe))
                {
                    masterList[recipe.Name] = recipe;
                    oldRecipes.Add(recipe);
                }
            }

            int oldCount = 0;
            do
            {
                oldCount = masterList.Count;
                newRecipes = new List<Recipe>();
                foreach (Recipe recipe in oldRecipes)
                {
                    foreach (RecipeIngredient ingredient in recipe.Ingredients)
                    {
                        Recipe intermediateRecipe;
                        if (this.Config.Recipes.TryGetValue(ingredient.Ingredient.Name, out intermediateRecipe) && intermediateRecipe.Name != recipe.Name)
                        {
                            masterList[ingredient.Ingredient.Name] = intermediateRecipe;
                            newRecipes.Add(intermediateRecipe);
                        }
                    }
                }

                oldRecipes = newRecipes;
            } while (oldCount < masterList.Count);

            this.RequiredRecipies = masterList.Values.ToList();
        }
    }
}
