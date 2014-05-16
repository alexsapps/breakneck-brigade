using BulletSharp;
using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCuisine
{
    class ServerGameController
    {
        protected ServerGame Game { get; set; }
        
        public List<ServerTeam> Teams { get; set; }

        public List<IngredientType> Goals { get; set; }

        public ServerGameController(ServerGame game)
        {
            this.Game = game;
            this.Teams = new List<ServerTeam>();
            this.Goals = new List<IngredientType>();
        }

        int ticks = 1; //server ticks, not time ticks

        public void Update()
        {
            if (ticks % 100 == 0)
                spawnIngredient(chooseIngredient(), RandomLocation());
            ticks++;
        }

        /*
         * chooses an ingredient to be spawned based on what ingredients are needed to make the goal recipes (goals).
         * chooses random ingredients if no ingredients are needed to make goals.
         */
        IngredientType chooseIngredient()
        {
            throw new NotImplementedException();
        }

        ServerIngredient spawnIngredient(IngredientType type, Vector3 location)
        {
            List<IngredientType> types = new List<IngredientType>(Game.Config.Ingredients.Values);
            IngredientType randIng = types[DC.random.Next(types.Count)];
            var loc = RandomLocation();
            return new ServerIngredient(randIng, Game, loc);
        }

        public static Vector3 RandomLocation()
        {
            return new Vector3(
                (float)Math.Pow(DC.random.Next(-800, 800), 1),
                (float)Math.Pow(DC.random.Next(10, 100), 2),
                (float)Math.Pow(DC.random.Next(-800, 800), 1));
        }
    }
}
