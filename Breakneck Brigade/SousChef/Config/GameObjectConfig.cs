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
                Recipes = new Dictionary<string, Recipe>(),
                Cookers = new Dictionary<string, CookerType>(),
                Terrains = new Dictionary<string,TerrainType>()
            };
            this.CurrentSalad = salad;

            const int nfiles = 4;
            string[] filenames = new string[nfiles];

            /*
             * must be loaded in order.
             * e.g. recipes parser requires this.CurrentSalad to have ingredients already loaded
             */
            foreach (var ingredient in new BBXIngredientsFileParser(this).LoadFile(out filenames[0]))
                salad.Ingredients.Add(ingredient.Name, ingredient);
            foreach (var recipe in new BBXRecipesFileParser(this).LoadFile(out filenames[1]))
                salad.Recipes.Add(recipe.Name, recipe);
            foreach (var cooker in new BBXCookersFileParser(this).LoadFile(out filenames[2]))
                salad.Cookers.Add(cooker.Name, cooker);
            foreach (var terrain in new BBXTerrainsFileParser(this).LoadFile(out filenames[3]))
                salad.Terrains.Add(terrain.Name, terrain);

            byte[][] hashes = new byte[nfiles][];
            for(int i = 0; i < nfiles; i++){
                hashes[i] = SHA512.GetChecksum(filenames[i]);
            }
            salad.Hash = SHA512.GetCombinedHash(hashes);

            this.CurrentSalad = null;
            return salad;
        }

    }
}