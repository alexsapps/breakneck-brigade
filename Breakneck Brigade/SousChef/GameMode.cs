using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public enum GameMode
    {
        None, //invalid / uninitialized game mode variable
        Init, //wait for users to connect
        Started, //the game is playing
        Paused, //the game is paused (may not be implemented)
        Results, //showing game results, winner/loser, final scores
        Stopping //the game is shutting down
    }
}
