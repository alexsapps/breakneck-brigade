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
        public Dictionary<IngredientType, int> NeededHash { get; set; } // the dict of the number of ingredients needed to make the goals 
        

        public int SpawnTick;
        private int SECONDSTOSPAWN = 1;
        private string[] defaultTeams = new string[]{"red", "blue"}; //Add more team names for more teams
        private Vector3 teamSpawn = new Vector3(400, 20, 0);


        private Dictionary<string,int> numOfGoalsByState; // dictionary conaining the number of goals
#if PROJECT_DEBUG
        private int pileSize = 5; // don't spawn ingredients
#else
        private int pileSize = 200;
#endif   
        public GameControllerState CurrentGameState { get; set; }
        
        public enum GameControllerState
        {
            Start,
            Waiting,
            Scatter,
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
        private int startTick = 30 * 7; // Debug, make waiting much shorter
        private int scatterTick = 30 * 5;
#else
        private int startTick = 30 * 7; // 5 seconds.
        private int scatterTick = 30 * 5;
#endif
        private bool _goalsDirty = false;

        public int ScoreToWin = 20000000;
        public long MaxTime = new TimeSpan(0,15,0).Ticks;

        public ServerGameController(ServerGame game)
        {
            this.Game = game;
            this.SpawnTick = 30 * SECONDSTOSPAWN;//game.FrameRateMilliseconds * SECONDSTOSPAWN; FrameRate not set, TODO:
            this.Teams = new Dictionary<string, ServerTeam>();
            foreach (string teamName in this.defaultTeams)
            {
                this.Teams.Add(teamName, new ServerTeam(teamName,teamSpawn ));
                teamSpawn.X *= -1; // the idea is that each team spawns at opposite ends. Only works cause we have 2 teams.
            }
            this.Goals = new List<Goal>();
            this.NeededHash = new Dictionary<IngredientType, int>();
            this.numOfGoalsByState = new Dictionary<string,int>();
            // TODO: read this from a file. For now we need gameplay. Also why can't I map a enum?
            this.numOfGoalsByState.Add(GameControllerState.Stage1.ToString(), 5);
            this.numOfGoalsByState.Add(GameControllerState.Stage2.ToString(), 7);
            this.numOfGoalsByState.Add(GameControllerState.Stage3.ToString(), 10);
        }

        private void FillGoals(int numOfGoals, int complexity)
        {
            int numOfRecipes = this.Game.Config.Recipes.Count();
            while (numOfGoals > Goals.Count)
            {
                // Grab random recipe
                var tmpRec = this.Game.Config.Recipes.ElementAt(DC.random.Next(0, numOfRecipes)).Value;
                // loop over ingredients and add them to the needed hash
                foreach (var ing in tmpRec.Ingredients)
                {
                    int count;
                    if (!this.NeededHash.TryGetValue(ing.Ingredient, out count))
                        this.NeededHash.Add(ing.Ingredient, 0); // initialize the dict
                    int i = 0;
                    while(i++ < ing.nCount + ing.nOptional)
                        this.NeededHash[ing.Ingredient]++;
                }
                Goals.Add(new Goal(100, tmpRec, complexity));
                _goalsDirty = true;
            }
        }

        public void UpdateConfig() 
        {
            if (CurrentGameState <= GameControllerState.Stage1)
                FillGoals(this.numOfGoalsByState[GameControllerState.Stage1.ToString()], 0);
            else if(CurrentGameState <= GameControllerState.Stage2)
                FillGoals(this.numOfGoalsByState[GameControllerState.Stage2.ToString()], 0);
            else
                FillGoals(this.numOfGoalsByState[GameControllerState.Stage3.ToString()], 0);
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
                    if (ticks == scatterTick)
                    {
                        risePile(); // gives a crazy scatter pile effect
                        nextGameState();
                    }
                    updateObj();
                    break;
                case GameControllerState.Scatter:
                    if (ticks == startTick)
                    {
                        scatterPile(); // gives a crazy scatter pile effect
                        nextGameState();
                    }
                    updateObj();
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
            Goal goalToRemove = null;
            foreach (var goal in Goals)
            {
                if (goal.EndGoal.FinalProduct.Name == ing.Type.Name)
                {
                    if (ing.LastPlayerHolding != null)
                    {
                        ing.LastPlayerHolding.Client.Team.Points += ((Goal)goal).Points;
                        ing.Remove();
                        MarkLobbyStateDirty();
                        goalToRemove = goal;
                        _goalsDirty = true;
                        Program.WriteLine("Scored " + ((Goal)goal).Points + " By making a " + ((Goal)goal).EndGoal.FinalProduct.Name + " for team " + ing.LastPlayerHolding.Client.Team.Name);
                        this.DisplayScore();
                        break;
                    }
                }
            }
            if (goalToRemove != null)
                this.Goals.Remove(goalToRemove);

        }
        public bool CheckWin()
        {
            List<ServerTeam> maxTeams = new List<ServerTeam>();
            foreach(var team in Teams.Values)
            {
                if (maxTeams.Count == 0 || team.Points == maxTeams[0].Points)
                    maxTeams.Add(team);
                else if(team.Points > maxTeams[0].Points)
                {
                    maxTeams.Clear();
                    maxTeams.Add(team);
                }
            }

            if (maxTeams[0].Points >= this.ScoreToWin || DateTime.Now.Subtract(Game.StartTime).Ticks > MaxTime)
            {
                if (maxTeams.Count == 1)
                    Game.Winner = maxTeams[0];
                else
                    Game.Winner = null; //draw
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
            foreach (var ing in this.NeededHash)
            {
                for (int x = 0; x < ing.Value; x++)
                {
                    var tmp = spawnIngredient(ing.Key, RandomLocation());
                    //tmp.Body.Gravity = new Vector3(0, 0, 0); // Don't have them start yet
                    //tmp.Body.ActivationState = ActivationState.ActiveTag;
                }
            }
            for (int x = 0; x < pileSize; x++)
            {
                var tmp = spawnIngredient(this.Game.Config.Ingredients.Values.ElementAt(DC.random.Next(this.Game.Config.Ingredients.Count)), RandomLocation());
                //tmp.Body.Gravity = new Vector3(0, 0, 0); // Don't have them start yet
                //tmp.Body.ActivationState = ActivationState.ActiveTag;
            }
        }

        private void scatterPile()
        {
            foreach (var obj in this.Game.GameObjects.Values)
            {
                if (obj.ObjectClass == GameObjectClass.Ingredient)
                {
                    // scatter only ingredients. Should be in the pile currently
                    obj.Body.LinearVelocity = randomVelocity(700);
                    obj.Body.Gravity = this.Game.World.Gravity;
                }
            }
        }

        private void checkGoals()
        {

        }

        private void risePile()
        {
            foreach (var obj in this.Game.GameObjects.Values)
            {
                if (obj.ObjectClass == GameObjectClass.Ingredient)
                {
                    // Raise the pile
                    obj.Body.LinearVelocity = new Vector3(0, DC.random.Next(300, 1000), 0);
                    obj.Body.Gravity = this.Game.World.Gravity;
                }
            }
        }


        /// <summary>
        /// Apply a randome velocity to an object. Y will be much less than all  the other
        /// </summary>
        private Vector3 randomVelocity(int max)
        {
            // Some weird logic to get a random large negative and positive number
            float[] xVel = new float[3]{DC.random.Next(max / 2, max),  DC.random.Next(-max, -(max / 2)), 0};
            //float yVel = DC.random.Next(max, max*4/3);
            float[] zVel = new float[3]{ DC.random.Next(max / 2, max), DC.random.Next(-max, -(max / 2)), 0, };
            
            Vector3 vel = new Vector3(xVel[DC.random.Next(0, 3)], 0, zVel[DC.random.Next(0, 3)]);
            if(vel.X == 0 && vel.Y == 0)
            {
                // randomize one of the velocities so it doens't go no where.
                switch(DC.random.Next(0,2)){
                    case(0):
                        vel.X = xVel[DC.random.Next(0, 2)]; // don't select the zero value
                        break;
                    case(1):
                        vel.Z = zVel[DC.random.Next(0, 2)]; // don't select the zero value
                        break;
                }
            }
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
