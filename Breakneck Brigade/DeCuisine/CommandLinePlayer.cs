using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

using BulletSharp;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace DeCuisine
{
    /// <summary>
    /// Class that allows the player to manipulate the game world through the command line
    /// </summary>
    static class CommandLinePlayer
    {

        /// <summary>
        /// read the arguments from the command line.
        /// </summary>
        /// <param name="args"></param>
        public static bool ReadArgs(string[] args, Server server, out string ret)
        {
            ret = null;
            bool success = true;
            switch (args[0])
            {
                // Everything under here is dev code. 
                case "add":
                    // "takes" two arguments, the first is the cooker id of where you want to 
                    // put the ingredient, the second is the ingredient id of what you want to add
                    if (args.Length < 3)
                    {
                        ret = "add expects at least two arguments.";
                        break;
                    }
                    lock (server.Lock)
                        lock (server.Game.Lock)
                            if(server.Game.Mode == GameMode.Started)
                                server.Game.TestCookerAdd(Convert.ToInt32(args[1]), Convert.ToInt32(args[2]));
                    break;
                case "listworld":
                    // list all objects ids in the game as well as there class 
                    lock (server.Lock)
                        lock(server.Game.Lock)
                            ret = server.Game.ListGameObjects();
                    break;
                case "monitor":
                    Thread m = new Thread(() => {
                        monitorThread(server);
                    });
                    m.Start();

                    break;
                case "listcooker":
                    // takes one argument, the cooker you want to list it's contents
                    if (args.Length < 2)
                    {
                        ret = "list cooker expects at least one argument.";
                        break;
                    }
                    lock (server.Lock)
                        lock(server.Game.Lock)
                            if (server.Game.Mode == GameMode.Started)
                                ret = server.Game.ListCookerContents(Convert.ToInt32(args[1]));
                    break;
                case "listing":
                    // lists all the ingredients by name in the game world
                    lock (server.Lock)
                        lock(server.Game.Lock)
                            if (server.Game.Mode == GameMode.Started)
                                ret = server.Game.ListIngredients();
                    break;
                case "spawn":
                    // spawn stuff, see function definition for right argument format
                    lock (server.Lock)
                        lock(server.Game.Lock)
                            if (server.Game.Mode == GameMode.Started)
                                Spawn(server.Game, args);
                    break;
                case "stresstest":
                    // spawn stuff, see function definition for right argument format
                    {
                        lock(server.Lock)
                            lock(server.Game.Lock)
                                if (server.Game.Mode == GameMode.Started)
                                {
                                    int n;
                                    if (args.Length < 2 || !int.TryParse(args[1], out n))
                                        n = 50;
                                    StressTest(server.Game, n);
                                }
                        break;
                    }
                case "remove":
                    {
                        lock (server.Lock)
                            lock (server.Game.Lock)
                                if (server.Game.Mode == GameMode.Started)
                                {
                                    int n;
                                    if (args.Length < 2 || !int.TryParse(args[1], out n))
                                    {
                                        Console.WriteLine("Needs an id");
                                        break;
                                    }
                                    server.Game.RemoveObj(n);
                                }
                        break;
                    }
                case "clear":
                    {
                        lock (server.Lock)
                            lock (server.Game.Lock)
                                if (server.Game.Mode == GameMode.Started)
                                {
                                    server.Game.ClearBoard();
                                }
                        break;
                    }
                default:
                    success = false;
                    break;
            }
            return success;
        }

        private static void monitorThread(Server server)
        {   
            while (!Program.cancelConsole)
            {
                lock (server.Lock)
                {
                    if (server.Game != null)
                    {
                        lock (server.Game.Lock)
                        {
                            Console.Clear();
                            Console.Write(server.Game.ListGameObjects());
                        }
                    }
                    else
                    {
                        Console.Clear();
                        Program.WriteLine("monitor is waiting for game to start");
                    }
                }
                System.Threading.Thread.Sleep(50);
            }
            Program.cancelConsole = false;
            Program.Prompt();
        }

        /// <summary>
        /// Technically, calls the collision function between the cooker and 
        /// ingredient. The collision function adds the ingredient to the cooker 
        /// list
        /// </summary>
        public static void TestCookerAdd(Dictionary<int,ServerGameObject> gameObjects, int cookerId, int ingredientId)
        {
            if (!sanityChecks(gameObjects, cookerId, GameObjectClass.Cooker))
                return;
            if (!sanityChecks(gameObjects, ingredientId, GameObjectClass.Ingredient))
                return;

            // final cooker specific check, is the ingredient alive?
            if (gameObjects[ingredientId].ToRender) // TODO: Don't use to render
                    gameObjects[cookerId].OnCollide(gameObjects[ingredientId]);
        }

        /// <summary>
        /// Lists all the game objects that exist in the world and various properties.
        /// </summary>
        public static string ListGameObjects(Dictionary<int, ServerGameObject> gameObjects)
        {
            if (gameObjects.Count > 0)
            {
                StringBuilder b = new StringBuilder();
                b.AppendLine("Object Id\tName\t\tLocation\t\tVelocity\t\tToRender");
                foreach (var x in gameObjects)
                    writeAttributes(x.Value, "", b);

                return b.ToString();
            }
            else
            {
                return "No objects\n";
            }
        }

        /// <summary>
        /// Lists only the ingredients in the world and various properties
        /// </summary>
        public static string ListIngredients(Dictionary<int, ServerGameObject> gameObjects)
        {
            StringBuilder b = new StringBuilder();
            b.AppendLine("Object Id " + "\t" + "Name" + "\t\t" + "Location" + "\t\t" + "ToRender");
            foreach (var x in gameObjects)
            {
                if (x.Value.ObjectClass == GameObjectClass.Ingredient)
                    writeAttributes(x.Value, "ing", b);
            }
            return b.ToString();
        }

        /// <summary>
        /// Lists the contents of the current cooker.
        /// </summary>
        public static string ListCookerContents(Dictionary<int,ServerGameObject> gameObjects, int cookerId)
        {
            StringBuilder b = new StringBuilder();
            if (!sanityChecks(gameObjects, cookerId, GameObjectClass.Cooker))
                return null; 

            // sanity checks passed list contents
            Program.WriteLine("Object Id " + "\t" + "Name");
            foreach (var x in ((ServerCooker)gameObjects[cookerId]).Contents)
            {
                b.AppendLine(x.Id + "\t\t" + x.Type.Name);
            }
            return b.ToString();
        }

        /// <summary>
        /// Stress test our system, AKA Spawn a metric shitload of things at once
        /// </summary>
        public static void StressTest(ServerGame game, int n)
        {
            int numOfCookers = 0;
            int numOfIngredients = n;
            for (int x = 0; x < numOfIngredients; x++ )
                SpawnIngredient(game);
            for (int x = 0; x < numOfCookers; x++)
                SpawnCooker(game);
        }

        /// <summary>
        /// processes spawn command arguments and calls the appropiate spawn functions
        /// </summary>
        public static string Spawn(ServerGame game, string[] args)
        {
            if (args.Length < 2) 
                return "You need to specify what type of object to spawn";
            else
            {
                switch (args[1])
                {
                    case "ingredient":
                        return spawnIngredientHelper(game, args);
                    case "cooker":
                        return spawnCookerHelper(game, args);
                    default:
                        return "WTF is a " + args[1] + ". I can't spawn that.";
                }
            }
        }
        public static string ClearBoard(Dictionary<int,ServerGameObject> gameObj)
        {
            StringBuilder b = new StringBuilder();
            int numCleared = 0;
            foreach (var x in gameObj)
            {
                if (x.Value.ObjectClass != GameObjectClass.Player &&
                    x.Value.ObjectClass != GameObjectClass.Plane)
                {
                    x.Value.Body.LinearVelocity = new Vector3(0, 5000, 5000);
                    numCleared++;
                }
                    
            }
            b.AppendLine("Cleared " + numCleared + " Objects");
            return b.ToString();
        }

        /// <summary>
        /// Clears the board in a dramatic fashion
        /// </summary>

        private static string spawnCookerHelper(ServerGame game, string[] args)
        {
            if (args.Length == 2) // spawn cooker
                return SpawnCooker(game);
            else if (args.Length == 3) // spawn cooker type
                return SpawnCooker(game, args[2]);
            else if (args.Length == 6) // spawn cooker type x y z
                return SpawnCooker(game, args[2], Convert.ToDouble(args[3]), Convert.ToDouble(args[4]), Convert.ToDouble(args[5]));
            else
                return "Spawn takes 1 arguments: one string that " +
                                  "that is the type of object you want to spawn. 2 arguments: the type of object and " +
                                  "the type of that object i.e. a banana. 5 arguments: the type of object and the " +
                                  "type of that object i.e. a blender and 3 doubles for the x, y and z spawn location";

        }

        private static string spawnIngredientHelper(ServerGame game, string[] args)
        {
            if (args.Length == 2) // spawn ingredient
                return SpawnIngredient(game);
            else if (args.Length == 3) // spawn ingredient type
                return SpawnIngredient(game, args[2]);
            else if (args.Length == 6) // spawn ingredient type x y z
                return SpawnIngredient(game, args[2], Convert.ToDouble(args[3]), Convert.ToDouble(args[4]), Convert.ToDouble(args[5]));
            else
                return "Spawn takes 1 arguments: one string that " +
                                  "that is the type of object you want to spawn. 2 arguments: the type of object and " +
                                  "the type of that object i.e. a banana. 5 arguments: the type of object and the " +
                                  "type of that object i.e. a blender and 3 doubles for the x, y and z spawn location";
        }

        /// <summary>
        /// Spawn a random ingredient at a random location.
        /// </summary>
        public static string SpawnIngredient(ServerGame game)
        {
            List<IngredientType> types = new List<IngredientType>(game.Config.Ingredients.Values);
            IngredientType randIng = types[DC.random.Next(types.Count)];
            Vector3 spawnLoc = randomSpawn();
            new ServerIngredient(randIng, game, spawnLoc);
            return "Made a " + randIng.Name + " at " + spawnLoc.X + " " + spawnLoc.Y + " " + spawnLoc.Z;
        }

        /// <summary>
        /// Spawn a the ingredient at a random location.
        /// </summary>
        public static string SpawnIngredient(ServerGame game, string type)
        {
            Vector3 spawnLoc = randomSpawn();
            new ServerIngredient(game.Config.Ingredients[type], game, spawnLoc);
            return "Made a " + game.Config.Ingredients[type].Name + " at " + spawnLoc.X + " " + spawnLoc.Y + " " + spawnLoc.Z;
        }

        /// <summary>
        /// Spawn an ingredient of the type passed  at the location. 
        /// </summary>
        public static string SpawnIngredient(ServerGame game, string type, double x, double y, double z)
        {
            Vector3 spawnLoc = new Vector3((float)x, (float)y, (float)z);
            new ServerIngredient(game.Config.Ingredients[type], game, spawnLoc);
            return "Made a " + game.Config.Ingredients[type].Name + " at " + spawnLoc.X + " " + spawnLoc.Y + " " + spawnLoc.Z;
        }

        /// <summary>
        /// Spawn a random ingredient at a random location.
        /// </summary>
        public static string SpawnCooker(ServerGame game)
        {
            List<CookerType> types = new List<CookerType>(game.Config.Cookers.Values);
            Random rand = new Random();
            CookerType randCooker = types[rand.Next(types.Count)];
            Vector3 spawnLoc = new Vector3(rand.Next(-500,500), rand.Next(300), rand.Next(-500, 500));
            new ServerCooker(randCooker, game, spawnLoc);
            return "Made a " + randCooker.Name + " at " + spawnLoc.X + " " + spawnLoc.Y + " " + spawnLoc.Z;
        }

        /// <summary>
        /// Spawn a the ingredient at a random location.
        /// </summary>
        public static string SpawnCooker(ServerGame game, string type)
        {
            Vector3 spawnLoc = randomSpawn();
            new ServerCooker(game.Config.Cookers[type], game, spawnLoc);
            return "Made a " + game.Config.Cookers[type].Name + " at " + spawnLoc.X + " " + spawnLoc.Y + " " + spawnLoc.Z;
        }

        /// <summary>
        /// Spawn an ingredient of the type passed  at the location. 
        /// </summary>
        public static string SpawnCooker(ServerGame game, string type, double x, double y, double z)
        {
            Vector3 spawnLoc = new Vector3((float)x, (float)y, (float)z);
            new ServerCooker(game.Config.Cookers[type], game, spawnLoc);
            return "Made a " + game.Config.Cookers[type].Name + " at " + spawnLoc.X + " " + spawnLoc.Y + " " + spawnLoc.Z;
        }

        /// <summary>
        /// helper to check if the id passed in is the right type and is in the dict.
        /// check yo self before you wreck self
        /// </summary>
        private static bool sanityChecks(Dictionary<int, ServerGameObject> gameObjects, int objId, GameObjectClass objType)
        {
            if (!gameObjects.ContainsKey(objId))
            {
                Program.WriteLine("Yo dawg, that Id " + objId + " ain't be in the world son");
                return false;
            }

            if (gameObjects[objId].ObjectClass != objType)
            {
                Program.WriteLine("Yo dawg, object at id " + objId + " Be a " + gameObjects[objId].ObjectClass + 
               ". That ain't a " + objType + " son.");
                return false;
            }
            return true;
        }

        

        /// <summary>
        /// helper to check if the config file has the requested ingredient type to make
        /// </summary>
        private static bool sanityChecks(string key, Dictionary<string, IngredientType> config)
        {
            if (!config.ContainsKey(key))
                return false;
            return true;
        }
        /// <summary>
        /// helper to randomly spawn objects
        /// </summary>
        /// <returns></returns>
        private static Vector3 randomSpawn()
        {
            return new Vector3(
                (float)Math.Pow(DC.random.Next(-800, 800), 1),
                (float)Math.Pow(DC.random.Next(10, 100), 2),
                (float)Math.Pow(DC.random.Next(-800, 800), 1))
            ;
        }

        /// <summary>
        /// Write them in a similar format. I can't format well though so there's that.
        /// </summary>
        private static string writeAttributes(ServerGameObject obj, string format, StringBuilder b)
        {
            switch(format)
            {
                case "ing":
                    b.AppendLine(obj.Id + "\t\t" + ((ServerIngredient)obj).Type.Name + "\t\t" + 
                                      (int)obj.Position.X + " " + (int)obj.Position.Y + " " + (int)obj.Position.Z +
                                      "\t\t" + obj.Body.LinearVelocity + "\t\t" + obj.ToRender);
                    break;
                default:
                    b.AppendLine(obj.Id + "\t\t" + obj.ObjectClass + "\t\t" + (int)obj.Position.X + " " + 
                                      (int)obj.Position.Y + " " + (int)obj.Position.Z + "\t\t" + obj.Body.LinearVelocity + "\t\t" + obj.ToRender);
                    break;
            }
            return b.ToString();
        }
    }
}
