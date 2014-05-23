using BulletSharp;
using SousChef;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCuisine
{
    class ServerGameController
    {
        protected ServerGame Game { get; set; }
        
        public Dictionary<string, ServerTeam> Teams { get; set; }

        public List<Goal> Goals { get; set; }



        public int SpawnTick;
        private int SECONDSTOSPAWN = 1;
        private string[] defaultTeams = new string[]{"red", "blue"}; //Add more team names for more teams

        private int _numGoals = 0;
        public int NumGoals
        {
            get
            {
                return _numGoals;
            }
            protected set
            {
                _numGoals = value;
                
                //remove or add goals until we have NumGoals goals
                while (value < Goals.Count)
                    Goals.RemoveAt(DC.random.Next(Goals.Count));
                FillGoals();
            }
        }

        private int ScoreToWin = 2000;

        WeightedRandomChooser<IngredientType> randomIngredientChooser;
        WeightedRandomChooser<IngredientType> randomGoalChooser;

        public ServerGameController(ServerGame game)
        {
               
            this.Game = game;
            this.SpawnTick = 30 * SECONDSTOSPAWN;//game.FrameRateMilliseconds * SECONDSTOSPAWN; FrameRate not set, TODO:
            this.Teams = new Dictionary<string, ServerTeam>();
            foreach (string teamName in this.defaultTeams)
            {
                this.Teams.Add(teamName, new ServerTeam(teamName));
            }
            this.Goals = new List<Goal>();
        }

        public void UpdateConfig(ConfigSalad salad, int numGoals) //must be called once before Update gets called
        {
            //ingredient worth more points => ingredient unlikely to be spawned
            randomIngredientChooser = new WeightedRandomChooser<IngredientType>(salad.Ingredients.Values, (ingr) => { return 1.0f / ingr.DefaultPoints; });

            //ingredient worth more points => ingredient more likely to be chosen as a goal ingredient
            randomGoalChooser = new WeightedRandomChooser<IngredientType>(salad.Ingredients.Values, (ingr) => { return ingr.DefaultPoints; });

            this.NumGoals = numGoals;
        }

        private void FillGoals()
        {
            while (NumGoals > Goals.Count)
            {
                IngredientType tmpIng = getWeightedRandomIngredient();
                Goals.Add(new Goal(100, tmpIng));
            }
        }

        int ticks = 1; //server ticks, not time ticks

        public void Update()
        {
            /*
             * handle an instant in time, e.g. gravity, collisions
             */
            foreach (var obj in new List<ServerGameObject>(Game.GameObjects.Values)) //allow removing items while enumerating
            {
                obj.Update();
            }

            if (ticks % this.SpawnTick == 0)
                spawnIngredient(getWeightedRandomGoal(), RandomLocation());

            ticks++;
        }

        /*
         * chooses an ingredient to be spawned based on what ingredients are needed to make the goal recipes (goals).
         * chooses random ingredients if no ingredients are needed to make goals.
         */
        IngredientType getWeightedRandomGoal()
        {
            return randomGoalChooser.Choose();
        }

        ServerIngredient spawnIngredient(IngredientType type, Vector3 location)
        {
            List<IngredientType> types = new List<IngredientType>(Game.Config.Ingredients.Values);
            IngredientType randIng = types[DC.random.Next(types.Count)];
            var loc = RandomLocation();
            return new ServerIngredient(randIng, Game, loc);
        }

        IngredientType getWeightedRandomIngredient()
        {
            return randomIngredientChooser.Choose();
        }

        public static Vector3 RandomLocation()
        {
            return new Vector3(
                (float)Math.Pow(DC.random.Next(-800, 800), 1),
                (float)Math.Pow(DC.random.Next(10, 100), 2),
                (float)Math.Pow(DC.random.Next(-800, 800), 1));
        }

        /// <summary>
        /// Find the team with the min number of players and assign the player to the team.
        /// Should be called at player creation. 
        /// </summary>
        public void AssignTeam(Client client)
        {
            Debug.Assert(Teams.Count > 0);

            // find the team with the lowest team number
            int minCount = int.MaxValue; 
            ServerTeam minTeam = null;

            foreach (var team in Teams.Values)
            {
                if (team.Members.Count < minCount)
                {
                    minCount = team.Members.Count;
                    minTeam = team;
                }
            }

            AssignTeam(client, minTeam);
        }

        /// <summary>
        /// Assign the player to the passed in team name. Currently not called by anything
        /// but somwhere we may want to have the ability to let players pick. 
        /// </summary>
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               /// <param name="player"></param> 
        /// <param name="teamName"></param>
        /// <returns></returns>
        public void AssignTeam(Client client, string teamName)
        {
            ServerTeam team;
            if (Teams.TryGetValue(teamName, out team))
            {
                AssignTeam(client, team);
            }
            else
            {
                throw new Exception("How the hell did we allow them to pick a team that doesn't exist?");
            }
        }

        public void AssignTeam(Client client, ServerTeam team)
        {
            if (client.Team != null)
                client.Team.Members.Remove(client);
            team.Members.Add(client);
            client.Team = team;
        }

        public void ScoreAdd(ServerPlayer player, ServerIngredient ing)
        {
            int points = ing.Type.DefaultPoints * ing.Cleanliness;
            player.Client.Team.Points += points;
            // TODO: Replace with call to gui or something
            Program.WriteLine("Player " + player.Id + " Scored " + points + " For " + player.Client.Team + " Team");
            this.DisplayScore();
            CheckWin(ing.LastPlayerHolding.Client.Team);
        }

        public void ScoreDeliver(ServerIngredient ing)
        {
            foreach (var goal in Goals)
            {
                if (goal.GoalIng.Name == ing.Type.Name)
                {
                    if (ing.LastPlayerHolding != null)
                    {
                        ing.LastPlayerHolding.Client.Team.Points += ((Goal)goal).Points;
                        ing.Remove();
                        Program.WriteLine("Scored " + ((Goal)goal).Points + " for team " + ing.LastPlayerHolding.Client.Team.Name);
                        this.DisplayScore();
                        CheckWin(ing.LastPlayerHolding.Client.Team);
                    }
                }
            }

        }
        public void CheckWin(ServerTeam team)
        {
            if (team.Points >= this.ScoreToWin)
            {
                Program.WriteLine("Team " + team.Name + " Wins!");
            }
        }

        public void DisplayScore()
        {
            foreach (var team in this.Teams)
            {
                Program.WriteLine(team.Value.Name + " Has " + team.Value.Points);
                //add individual scores as well.
            }
                
        }
        /// <summary>
        /// Class which facilitates choosing items randomly based on their weights.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public delegate float WeightGetter<T>(T item);
        public class WeightedRandomChooser<T>
        {
            struct weightedItem
            {
                public float cumulativeWeight;
                public T obj;
            }
            List<weightedItem> wItems = new List<weightedItem>();
            float cumWeight;
            public WeightedRandomChooser(IEnumerable<T> items, WeightGetter<T> weighGetter)
            {
                float cumWeight = 0;
                foreach(var item in items)
                {
                    cumWeight += weighGetter(item);
                    wItems.Add(new weightedItem()
                    {
                         cumulativeWeight = cumWeight,
                         obj = item
                    });
                }
                this.cumWeight = cumWeight;
            }
            public T Choose()
            {
                float r = (float)(DC.random.NextDouble() * cumWeight);
                return find(0, wItems.Count, r);
            }

            private T find(int min_i, int max_i, float r)
            {
                for (int i = min_i; i < max_i; i++)
                {
                    if (r <= wItems[i].cumulativeWeight)
                    {
                        return wItems[i].obj;
                    }
                }
                throw new Exception("WeightedRandomChooser find bug");
            }
        }
    }
}
