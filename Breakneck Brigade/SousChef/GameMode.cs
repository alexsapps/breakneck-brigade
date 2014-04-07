using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public enum GameMode
    {
        Init, //wait for users to connect
        Started, //the game is playing
        Paused, //the game is paused (may not be implemented)
        Stopping //the game is shutting down
    }
}
