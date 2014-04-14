using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SousChef
{
    public class GameObjectType
    {
        public string Name { get; set; }

        public GameObjectType()
        {

        }

        public GameObjectType(string name)
        {
            this.Name = name;
        }
    }
}
