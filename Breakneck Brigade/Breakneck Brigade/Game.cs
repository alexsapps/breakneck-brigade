using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breakneck_Brigade
{
    class Game
    {
        public BBLock Lock = new BBLock();

        public Dictionary<string, string> gameObjects { get; private set; }

        public Game()
        {
            gameObjects = new Dictionary<string, string>();
        }

    }
}
