using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SousChef
{
    public delegate int IdGetter();

    public class GameObjectConfig
    {
        public IdGetter IdGetter { get; private set; }
        public ConfigSalad CurrentSalad { get; private set; }
        public string ConfigDir { get; private set; }

        public GameObjectConfig() 
            : this(BBXml.GetAppConfigFolder())
        {
            
        }
        public GameObjectConfig(string configDir)
        {
            this.ConfigDir = configDir;
        }

        public delegate List<T> BBXListHandler<T>(XmlReader node);
        public delegate T BBXItemHandler<T>(XmlReader node);

        public ConfigSalad GetConfigSalad()
        {
            ConfigSalad salad = new ConfigSalad()
            {
                Ingredients = new Dictionary<string, IngredientType>(),
                Recipies = new Dictionary<string, Recipe>(),
                Cookers = new Dictionary<string, CookerType>()
            };
            this.CurrentSalad = salad;

            /*
             * must be loaded in order.
             * e.g. recipes parser requires this.CurrentSalad to have ingredients already loaded
             */
            foreach (var ingredient in new BBXIngredientsFileParser(this).LoadFile())
                salad.Ingredients.Add(ingredient.Name, ingredient);
            foreach (var recipe in new BBXRecipesFileParser(this).LoadFile())
                salad.Recipies.Add(recipe.Name, recipe);
            foreach (var cooker in new BBXCookersFileParser(this).LoadFile())
                salad.Cookers.Add(cooker.Name, cooker);

            this.CurrentSalad = null;
            return salad;
        }

    }
}