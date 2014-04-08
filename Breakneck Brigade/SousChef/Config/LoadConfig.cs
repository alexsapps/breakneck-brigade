using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace SousChef
{
    public class LoadConfig
    {
        public Dictionary<string, string> configFiles;
        public struct ConfigSalad
        {
            public List<Ingredient> ingredients;
            public Dictionary<string, Recipe> recipies;
            //public Dictionary<string, Cooker> cookers;
        }

        /*
         * Loads the configuartion file. 
         * configFolder = the config folder where the configFile and all other xml is stored
         * configFile = A xml file containging the names of the individual config files
         */
        public LoadConfig(string configFolder, string configFile)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(configFolder + configFile);
            configFiles = new Dictionary<string, string>();
            foreach(XmlNode node in doc.DocumentElement.ChildNodes)
            {
                string fileName = node.SelectSingleNode("filename").InnerText.Trim();
                string id = node.SelectSingleNode("id").InnerText.Trim();
                configFiles.Add(id, configFolder + fileName); //include full path
            }
        }

        public ConfigSalad LoadAll()
        {
            ConfigSalad salad = new ConfigSalad();
            salad.ingredients = LoadIngredientsFile();
            //salad.recipies = LoadRecipiesFile();
            return salad;
        }

        public List<Ingredient> LoadIngredientsFile()
        {
            List<Ingredient> ingredients = new List<Ingredient>();
            XmlDocument doc = new XmlDocument();
            doc.Load(configFiles["ingredients"]);

            //loop over all the ingredient nodes
            foreach(XmlNode node in doc.DocumentElement.ChildNodes)
            {
                string name = node.SelectSingleNode("name").InnerText.Trim();
                Ingredient ingToAdd = new Ingredient(1, name);
                string model = node.SelectSingleNode("model").InnerText.Trim();
                ingredients.Add(ingToAdd);
            }
            return ingredients;
        }

        public Dictionary<string, string> LoadCookerFile()
        {
            return configFiles;
        }

        public Dictionary<string, Recipe> LoadRecipiesFile()
        {
            Dictionary<string, Recipe> recipes = new Dictionary<string, Recipe>();
            XmlDocument doc = new XmlDocument();
            doc.Load(configFiles["recipes"]);

            //loop ovewr recipe nodes
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                List<Ingredient> ingredients = new List<Ingredient>();
                string name = node.SelectSingleNode("name").InnerText.Trim();
                Ingredient finalProduct = new Ingredient(1, name);
                string cooker = node.SelectSingleNode("cooker").InnerText.Trim();

                //select all innner ingredient nodes
                XmlNodeList ingredientsNodes = node.SelectNodes("ingredients");
                foreach (XmlNode ingredientNode in ingredientsNodes)
                {
                    string ing = ingredientNode.InnerText.Trim();
                    Ingredient ingToAdd = new Ingredient(1, ing);
                    ingredients.Add(ingToAdd);
                }

                Recipe recipesToAdd = new Recipe(ingredients, cooker, finalProduct, "place");
                recipes.Add(name, recipesToAdd);
            }

            return recipes;
        }
    }
}
