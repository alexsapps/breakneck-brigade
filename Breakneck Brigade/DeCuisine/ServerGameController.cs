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
        public Dictionary<string,string> TintList { get; set; }
        

        public int SpawnTick;
        private int SECONDSTOSPAWN = 1;
        private string[] defaultTeams = new string[]{"red", "blue"}; //Add more team names for more teams
        private Vector3 teamSpawn = new Vector3(400, 20, 0);



        private int pileSize = 200;
        
        public GameControllerState CurrentGameState { get; set; }
        
        public enum GameControllerState
        {
            Start,
            Waiting,
            Stage1,
            Stage2,
            Stage3,
            End
        }
        protected void nextGameState()
        {
            CurrentGameState = (GameControllerState)((int)CurrentGameState + 1);
        }

#if PROJECT_DEBUG
        private int startTick = 3;//0 * 5; // Debug, make waiting much shorter
#else
        private int startTick = 30 * 5; // 5 seconds.
#endif

        private int _numGoals = 0;
        private bool _goalsDirty = false;
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
                if (value < Goals.Count)
                {
                    do
                        Goals.RemoveAt(DC.random.Next(Goals.Count));
                    while (value < Goals.Count);

                    _goalsDirty = true;
                }
                FillGoals();
            }
        }

        public int ScoreToWin = 20000000;
        public long MaxTime = new TimeSpan(0,15,0).Ticks;

        WeightedRandomChooser<IngredientType> randomIngredientChooser;
        WeightedRandomChooser<IngredientType> randomGoalChooser;

        public ServerGameController(ServerGame game)
        {
            this.Game = game;
            this.SpawnTick = 30 * SECONDSTOSPAWN;//game.FrameRateMilliseconds * SECONDSTOSPAWN; FrameRate not set, TODO:
            this.Teams = new Dictionary<string, ServerTeam>();
            foreach (string teamName in this.defaultTeams)
            {
                this.Teams.Add(teamName, new ServerTeam(teamName,teamSpawn ));
                teamSpawn.X *= -1; // the idea is that each team spawns at opposite ends. Only works cause we have 2 teams
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
                _goalsDirty = true;
            }
        }

        int ticks = 1; //server ticks, not time ticks

        public bool Update()
        {
            switch (CurrentGameState)
            {
                case GameControllerState.Start:
                    spawnPile();
                    nextGameState();
                    break;
                case GameControllerState.Waiting:
                    if (ticks == startTick)
                        nextGameState();
                    updateObj();
                    scatterPile(); // gives a crazy scatter pile effect
                    break;
                case GameControllerState.Stage1:
                    updateObj();
                    break;
            }
           
            if (_lobbyStateDirty)
            {
                _lobbyStateDirty = false;
                Game.SendLobbyStateToAll();
            }
            if(_goalsDirty)
            {
                _goalsDirty = false;
                Game.SendGoalsUpdate();
            }

            if (CheckWin())
                return false;
            
            ticks++;
            return true;
        }

        private void updateObj()
        {
            foreach (var obj in new List<ServerGameObject>(Game.GameObjects.Values)) //allow removing items while enumerating
            {
                obj.Update();
            }

        }
        /*
         * chooses an ingredient to be spawned based on what ingredients are needed to make the goal recipes (goals).
         * chooses random ingredients if no ingredients are needed to make goals.
         */
        IngredientType getWeightedRandomGoal()
        {
            // the goal must be the final product of a recipe, because if it is a leaf ingredient, it can't be made (because it's a leaf) and goals are never spawned

            IngredientType goal;
            
            goal = randomGoalChooser.Choose();

            foreach (var recipe in Game.Config.Recipies.Values)
                if (goal == recipe.FinalProduct)
                    return goal;

            return getWeightedRandomGoal(); //try again
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
            IngredientType result;

            result = randomIngredientChooser.Choose();

            // we don't want to spawn the goals.  make sure we didn't.
            foreach (var goal in Goals)
                if (goal.GoalIng == result)
                    return getWeightedRandomIngredient(); //try again

            return result;
        }

        public static Vector3 RandomLocation()
        {
            return new Vector3(
                (float)Math.Pow(DC.random.Next(-20, 20), 1),
                (float)Math.Pow(DC.random.Next(2, 5), 2),
                (float)Math.Pow(DC.random.Next(-20, 20), 1));
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
                if (team.GetMembers().Count < minCount)
                {
                    minCount = team.GetMembers().Count;
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
                client.Team.RemoveMember(client);
            team.AddMember(client);
            client.Team = team;
            MarkLobbyStateDirty();
        }

        public void ScoreAdd(ServerPlayer player, ServerIngredient ing)
        {
            int points = ing.Type.DefaultPoints * ing.Cleanliness;
            player.Client.Team.Points += points;
            MarkLobbyStateDirty();

            // TODO: Replace with call to gui or something
            Program.WriteLine("Player " + player.Id + " Scored " + points + " For " + player.Client.Team + " Team");
            this.DisplayScore();
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
                        MarkLobbyStateDirty();

                        Program.WriteLine("Scored " + ((Goal)goal).Points + " for team " + ing.LastPlayerHolding.Client.Team.Name);
                        this.DisplayScore();
                    }
                }
            }

        }
        public bool CheckWin()
        {
            ServerTeam maxTeam = null;
            foreach(var team in Teams.Values)
            {
                if(maxTeam == null || team.Points > maxTeam.Points)
                {
                    maxTeam = team;
                }
            }

            if (maxTeam.Points >= this.ScoreToWin || DateTime.Now.Subtract(Game.StartTime).Ticks > MaxTime)
            {
                Game.Winner = maxTeam;
                return true;
            }

            return false;
            }

        bool _lobbyStateDirty = false;
        void MarkLobbyStateDirty()
        {
            _lobbyStateDirty = true;
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
        /// Assign a spawn point to a random place
        /// </summary>
        public Vector3 AssignSpawnPoint(Client client)
        {
            Vector3 spawnLoc = client.Team.SpawnPoint;
            spawnLoc.X += DC.random.Next(15);
            spawnLoc.Z += DC.random.Next(15);
            return spawnLoc;
        }

        /// <summary>
        /// Spawn the cornicopia(sp)
        /// </summary>
        private void spawnPile()
        {
            for (int x = 0; x < pileSize; x++)
            {
                var tmp = spawnIngredient(getWeightedRandomGoal(), RandomLocation());
                tmp.Body.Gravity = new Vector3(0, 0, 0); // Don't have them start yet
            }
        }

        private void scatterPile()
        {
            foreach (var obj in this.Game.GameObjects.Values)
            {
                if (obj.ObjectClass == GameObjectClass.Ingredient)
                {
                    // scatter only ingredients. SHould be in the pile currently
                    obj.Body.LinearVelocity = randomVelocity(200);
                    obj.Body.Gravity = this.Game.World.Gravity;
                }
            }
        }


        /// <summary>
        /// Apply a randome velocity to an object. Y will be much less than all  the other
        /// </summary>
        private Vector3 randomVelocity(int max)
        {
            Vector3 vel = new Vector3(DC.random.Next(-max, max), DC.random.Next(max), DC.random.Next(-max, max));
            return vel;
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
