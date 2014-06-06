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
        public Dictionary<IngredientType, int> NeededCounts { get; set; } // the dict of the number of ingredients needed to make the goals 

        private Dictionary<ServerIngredient, double> finishedProdHash; // Hash that holds the currently finished product in the game world
        private int ticks; //server ticks, not time ticks
        public int SpawnTick;
        private int SECONDSTOSPAWN = 1;
        private string[] defaultTeams = new string[]{"blue", "red"}; //Add more team names for more teams
        private Vector3 teamSpawn = new Vector3(400, 20, 0);
        private int stageNum = 1;
        private ServerIngredient powerItem{get; set;}


        private Dictionary<GameControllerStage, int> numOfGoalsByState; // dictionary conaining the number of goals
#if PROJECT_WORLD_BUILDING
        private int pileSize = 0; // don't spawn ingredients
#else
        private int pileSize = 200;
#endif   
        public GameControllerState CurrentGameState { get; set; }
        public GameControllerStage CurrentStage { get; set; }
        
        public enum GameControllerState
        {
            Start,
            Waiting,
            Scatter,
            Play,
            End
        }

        public enum GameControllerStage
        {
            Stage1,
            Stage2,
            Stage3
        }
        protected void nextGameState()
        {
            CurrentGameState = (GameControllerState)((int)CurrentGameState + 1);
            // advanced to next stage if we aren't at the end
            if (CurrentGameState == GameControllerState.End) // && CurrentStage != GameControllerStage.Stage3)
            {
                // CurrentStage = (GameControllerStage)((int)CurrentStage + 1);
                this.CurrentGameState = GameControllerState.Start;
                // FillGoals();
                this.FillGoals(stageNum, 0);
                this.stageNum++;
                this.ticks = 0;
            }
        }



#if PROJECT_WORLD_BUILDING
        private int startTick = 30 * 2; // Don't care about pile when buliding the world
        private int scatterTick = 30 * 1;
#else
        private int startTick = 30 * 7; // 5 seconds.
        private int scatterTick = 30 * 5;
#endif
        private bool _goalsDirty = false;

        public int ScoreToWin = 20000000;
        public readonly long MaxTime = new TimeSpan(0,5,0).Ticks;

        public ServerGameController(ServerGame game)
        {
            this.Game = game;
            finishedProdHash = new Dictionary<ServerIngredient, double>();
            this.numOfGoalsByState = new Dictionary<GameControllerStage,int>();
            // TODO: read this from a file. For now we need gameplay. Also why can't I map a enum?
            this.numOfGoalsByState.Add(GameControllerStage.Stage1, 1);
            this.numOfGoalsByState.Add(GameControllerStage.Stage2, 2);
            this.numOfGoalsByState.Add(GameControllerStage.Stage3, 3);
            this.ResetGame();
            this.powerItem = null;
        }

        /// <summary>
        /// Initializes everything for the game.
        /// </summary>
        private void ResetGame()
        {
            this.SpawnTick = 30 * SECONDSTOSPAWN;//game.FrameRateMilliseconds * SECONDSTOSPAWN; FrameRate not set, TODO:
            this.Teams = new Dictionary<string, ServerTeam>();
            foreach (string teamName in this.defaultTeams)
            {
                this.Teams.Add(teamName, new ServerTeam(teamName, teamSpawn));
                teamSpawn.X *= -1; // the idea is that each team spawns at opposite ends. Only works cause we have 2 teams.
            }
            this.Goals = new List<Goal>();
            this.NeededCounts = new Dictionary<IngredientType, int>();
            this.ticks = 0;
            this.stageNum = 1;
        }

        private void FillGoals()
        {
            if (CurrentStage == GameControllerStage.Stage1)
                FillGoals(this.numOfGoalsByState[GameControllerStage.Stage1], 0);
            else if (CurrentStage == GameControllerStage.Stage2)
                FillGoals(this.numOfGoalsByState[GameControllerStage.Stage2], 0);
            else
                FillGoals(this.numOfGoalsByState[GameControllerStage.Stage3], 0);
        }


        /// <summary>
        /// Get the list of goals needed for the round.
        /// </summary>
        /// <param name="numOfGoals"></param>
        /// <param name="complexity"></param>
        private void FillGoals(int numOfGoals, int complexity)
        {
#if !PROJECT_WORLD_BUILDING
            int numOfRecipes = this.Game.Config.Recipes.Count();
            while (numOfGoals > Goals.Count)
            {
                // Grab random recipe
                Recipe recipe;
                do
                {
                    recipe = this.Game.Config.Recipes.ElementAt(DC.random.Next(0, numOfRecipes)).Value;
                } while (recipe.intermediate);
                // loop over ingredients and add them to the needed hash
                foreach (var ing in recipe.Ingredients)
                {
                    if (ing.Ingredient == recipe.FinalProduct)
                        continue; // stacked object, i.e. cake, don't spawn it
                    putInNeededCounts(ing);
                }
                Goals.Add(new Goal(recipe.FinalProduct.DefaultPoints, recipe, complexity));
                _goalsDirty = true;
            }
#endif
        }

        /// <summary>
        /// Check if the item you need to hash is also a recipe final product and spawns 
        /// the needed components to make that.
        /// </summary>
        private void putInNeededCounts(RecipeIngredient recIng)
        {
            // You know how we spend so much time learning recursion? This is the only instance
            // in this game
            int multiplicity = Teams.Count;
            if (this.Game.Config.Recipes.ContainsKey(recIng.Ingredient.Name))
            {
                foreach (var ingRec in this.Game.Config.Recipes[recIng.Ingredient.Name].Ingredients)
                    putInNeededCounts(ingRec);
            }
            else
            {
                if (this.NeededCounts.ContainsKey(recIng.Ingredient))
                    this.NeededCounts[recIng.Ingredient] += recIng.nCount * multiplicity;
                else
                    this.NeededCounts[recIng.Ingredient] = recIng.nCount * multiplicity;
            }
        }

        public void UpdateConfig() 
        {
            FillGoals();
        }

        public bool Update()
        {
            switch (CurrentGameState)
            {
                case GameControllerState.Start:
                    Game.SendSound(BBSound.warningbuzz, new Vector3(0, 10, 0));
                    spawnPile();
                    nextGameState();
                    break;
                case GameControllerState.Waiting:
                    if (ticks == scatterTick)
                    {
                        risePile(); // gives a crazy scatter pile effect
                        nextGameState();
                        Game.SendSound(BBSound.phasein, new Vector3(0, 10, 0));
                    }
                    updateObj();
                    break;
                case GameControllerState.Scatter:
                    if (ticks == startTick)
                    {
                        Game.SendSound(BBSound.explosionfar, new Vector3(0, 10, 0));
                        scatterPile(); // gives a crazy scatter pile effect
                        nextGameState();
                    }
                    updateObj();
                    break;
                case GameControllerState.Play:
                    updateObj();
                    break;
                case GameControllerState.End:
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
            
            this.ticks++;
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
            return new ServerIngredient(type, Game, location);
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
        public void AssignTeam(Client client, string teamName)
        {
            ServerTeam team;
            if (Teams.TryGetValue(teamName, out team))
                AssignTeam(client, team);
            else
                throw new Exception("How the hell did we allow them to pick a team that doesn't exist?");
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

        public bool ScoreDeliver(ServerIngredient ing)
        {
            Goal goalToRemove = null;
            foreach (var goal in Goals)
            {
                if (goal.EndGoal.FinalProduct.Name == ing.Type.Name)
                {
                    if (ing.LastPlayerHolding != null)
                    {
                        Game.SendParticleEffect(BBParticleEffect.CONFETTI, ing.LastPlayerHolding.Position, 0, ing.LastPlayerHolding.Id);
                        double complexity = 0;
                        ing.LastPlayerHolding.Client.Team.Points += (int)(((Goal)goal).Points + (1 + complexity));
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
            {
                this.Goals.Remove(goalToRemove);
                if (this.Goals.Count == 0)
                    this.nextGameState();
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Function to add the cooked objects ingredients to keep track of the items
        /// that went into the ingredient. Should only be called after a cook event.
        /// </summary>
        /// <param name="finalProduct">The product you made</param>
        /// <param name="components">All of the ingredients that made the Product.</param>
        public void FinishCook(ServerIngredient finalProduct, List<ServerIngredient> components, double complexity, ServerTeam team)
        {
            List<IngredientType> tmpList = new List<IngredientType>();
            foreach (var comp in components)
            {
                if (finishedProdHash.ContainsKey(comp))
                {
                    // you used the finished product so remove it.
                    complexity += finishedProdHash[comp];
                    finishedProdHash.Remove(comp);
                }
            }
            team.Points += (int)(finalProduct.Type.DefaultPoints * (1 + complexity)); 
            finishedProdHash.Add(finalProduct, complexity);
            this.Game.SendLobbyStateToAll(); // update client scores
        }

        public bool CheckWin()
        {
#if PROJECT_WORLD_BUILDING
            return false;       
#endif
            List<ServerTeam> maxTeams = new List<ServerTeam>();
            foreach (var team in Teams.Values)
            {
                if (maxTeams.Count == 0 || team.Points == maxTeams[0].Points)
                    maxTeams.Add(team);
                else if (team.Points > maxTeams[0].Points)
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
            HashSet<IngredientType> goalIng = new HashSet<IngredientType>();
            foreach(var goal in this.Goals)
                goalIng.Add(goal.EndGoal.FinalProduct);
            foreach (var ing in this.NeededCounts)
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
                var tmpIngType =  this.Game.Config.Ingredients.Values.ElementAt(DC.random.Next(this.Game.Config.Ingredients.Count));
                if (goalIng.Contains(tmpIngType))
                    continue;
                else if (this.Game.Config.Recipes.ContainsKey(tmpIngType.Name))
                    continue;
                var tmp = spawnIngredient(tmpIngType, RandomLocation());
                //tmp.Body.Gravity = new Vector3(0, 0, 0); // Don't have them start yet
                //tmp.Body.ActivationState = ActivationState.ActiveTag;
            }
        }

        private void scatterPile()
        {
#if PROJECT_WORLD_BUILDING
            return;
#endif
            foreach (var obj in this.Game.GameObjects.Values)
            {
                if (obj.ObjectClass == GameObjectClass.Ingredient)
                {
                    // scatter only ingredients. Should be in the pile currently
                    var velocity = randomVelocity(26);
                    velocity.X = (float)Math.Pow(velocity.X, 2.0) * Math.Sign(velocity.X);
                    velocity.Y = (float)Math.Pow(velocity.Y, 2.0) * Math.Sign(velocity.Y);
                    velocity.Z = (float)Math.Pow(velocity.Z, 2.0) * Math.Sign(velocity.Z);
                    obj.Body.LinearVelocity = velocity;
                    obj.Body.Gravity = this.Game.World.Gravity;
                }
            }
        }

        private void checkGoals()
        {

        }

        private void risePile()
        {
#if PROJECT_WORLD_BUILDING
            return;
#endif
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
            float[] xVel = new float[3]{DC.random.Next(0, max),  DC.random.Next(-max, -(max)), 0};
            //float yVel = DC.random.Next(max, max*4/3);
            float[] zVel = new float[3]{ DC.random.Next( 0, max), DC.random.Next(-max, -(max)), 0, };
            
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
        /// Return all the recipe for the current goals.
        /// </summary>
        /// <param name="team"></param>
        public List<Recipe> getGoalRecipeList()
        {
            List<Recipe> tmpRecipes = new List<Recipe>();

            foreach (Goal goal in this.Goals)
            {
                tmpRecipes.AddRange(recurseDownRecipe(goal.EndGoal));
            }
            return tmpRecipes;
        }

        /// <summary>
        /// Recurses down the recipe list to add only inermidiate steps
        /// </summary>
        /// <param name="rec"></param>
        /// <returns></returns>
        List<Recipe> recurseDownRecipe(Recipe rec)
        {
            List<Recipe> recList = new List<Recipe>();
            recList.Add(rec); // add the current recipe
            foreach (var ingredientType in rec.Ingredients)
            {
                if(this.Game.Config.Recipes.ContainsKey(ingredientType.Ingredient.Name))
                    recList.AddRange(recurseDownRecipe(this.Game.Config.Recipes[ingredientType.Ingredient.Name]));
            }
            return recList;
        }

        /// <summary>
        /// Power up an ingredient
        /// </summary>
        public void powerUpItem(ServerIngredient powUpIng, ServerStaticObject powerWindow)
        {
            double tmp;
            if(powUpIng.Type.powerUp == 0)
            {
                if(!this.finishedProdHash.TryGetValue(powUpIng, out tmp)) 
                    return; // not a finished product, can't power it up
                if (powUpIng == this.powerItem)
                    return;
                this.powerItem = powUpIng;
                powUpIng.ToRender = false; // hide the object
                powUpIng.Removed += ingredient_Removed;
            }
            else
            {
                if (this.powerItem == null)
                    return; // need the power item first.
                Vector3 ingredientSpawningPoint = new Vector3(powerWindow.Position.X, powerWindow.Position.Y + powerWindow.GeomInfo.Size[1], powerWindow.Position.Z); // spawn above cooker for now TODO: Logically spawn depeding on cooker
                powerItem.Body.ApplyImpulse(new Vector3(0, 300, 0), ingredientSpawningPoint);
                powerItem.ToRender = true;
                if (!this.finishedProdHash.TryGetValue(powerItem, out tmp))
                    throw new Exception("Something is wrong with the powering up items"); // not a finished product, can't power it up
                this.finishedProdHash[powerItem] += powUpIng.Type.powerUp;
                Game.SendParticleEffect(BBParticleEffect.STARS, powerItem.Position, 0, powerItem.Id);
                powUpIng.Remove();
            }
        }

        // happens when an ingredient is removed from the world
        private void ingredient_Removed(object sender, EventArgs e)
        {
            var ingredient = ((ServerIngredient)sender);
            if (this.powerItem == ingredient)
                this.powerItem = null;
            ingredient.Removed -= ingredient_Removed;
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
