using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

namespace DeCuisine
{
    /// <summary>
    /// Class that allows the player to manipulate the game world through the command line
    /// </summary>
    static class CommandLinePlayer
    {
        /// <summary>
        /// Technically, calls the collision function between the cooker and 
        /// ingredient. The collision function adds the ingredient to the cooker 
        /// list
        /// </summary>
        public static void TestCookerAdd(Dictionary<int,ServerGameObject> gameObjects, int cookerId, int ingredientId)
        {
            if (!SanityChecks(gameObjects, cookerId, GameObjectClass.Cooker))
                return;
            if (!SanityChecks(gameObjects, ingredientId, GameObjectClass.Ingredient))
                return;

            // final cooker specific check, is the ingredient alive?
            if (gameObjects[ingredientId].ToRender) // TODO: Don't use to render
                    gameObjects[cookerId].OnCollide(gameObjects[ingredientId]);
        }

        /// <summary>
        /// Lists all the game objects that exist in the world and various properties.
        /// </summary>
        public static void ListGameObjects(Dictionary<int, ServerGameObject> gameObjects)
        {
            Console.WriteLine("Object Id " + "\t" + "Name");
            foreach (var x in gameObjects)
            {
                Console.WriteLine(x.Key + "\t\t" + x.Value.ObjectClass);
            }
        }

        /// <summary>
        /// Lists only the ingredients in the world and various properties
        /// </summary>
        public static void ListIngredients(Dictionary<int, ServerGameObject> gameObjects)
        {
            Console.WriteLine("Object Id " + "\t" + "Name");
            foreach (var x in gameObjects)
            {
                if (x.Value.ObjectClass == GameObjectClass.Ingredient)
                    Console.WriteLine(x.Key + "\t\t" + ((ServerIngredient)x.Value).Type.Name);
            }
        }

        /// <summary>
        /// Lists the contents of the current cooker.
        /// </summary>
        public static void ListCookerContents(Dictionary<int,ServerGameObject> gameObjects, int cookerId)
        {
            if (!SanityChecks(gameObjects, cookerId, GameObjectClass.Cooker))
                return; 

            // sanity checks passed list contents
            Console.WriteLine("Object Id " + "\t" + "Name");
            foreach (var x in ((ServerCooker)gameObjects[cookerId]).Contents)
            {
                Console.WriteLine(x.Id + "\t\t" + x.Type.Name);
            }
        }


        // helper to check if the id passed in is the right type and is in the dict.
        // check yo self before you wreck self
        private static bool SanityChecks(Dictionary<int, ServerGameObject> gameObjects, int objId, GameObjectClass objType)
        {
            if (!gameObjects.ContainsKey(objId))
            {
                Console.WriteLine("Yo dawg, that Id " + objId + " ain't be in the world son");
                return false;
            }

            if (gameObjects[objId].ObjectClass != objType)
            {
                Console.WriteLine("Yo dawg, object at id " + objId + " Be a " + gameObjects[objId].ObjectClass + 
               ". That ain't a " + objType + " son.");
                return false;
            }
            return true;
        }
    }
}
