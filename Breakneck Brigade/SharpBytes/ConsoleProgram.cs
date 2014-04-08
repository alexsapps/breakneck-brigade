using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

namespace SharpBytes
{
    class ConsoleProgram
    {
        static void Main(string[] args)
        {
            string configLocation = "../../Config/";
            LoadConfig configLoader = new LoadConfig(configLocation, "ConfigFiles.xml");
            List<string> ingredients = configLoader.LoadIngredientsFile();
            Dictionary<string, Recipe> recipe = configLoader.LoadRecipiesFile();
            Console.Write("breakpoint"); //set break point here to stop program
        }
    }
}
