using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Breakneck_Brigade
{
    abstract class GameObject
    {
        public int id { get; set; }
        public string tag { get; set; }

        public GameObject(int id)
        {
            this.id = id;
            //this.pos = vector(0,0,0)
        }

        public GameObject(int id, string tag)
            : this(id)
        {
            this.tag = tag;
            //this.pos = vector(0,0,0)
        }

        /* public GameObject(int id, string tag, vector position)
         * : this(id, tag)
        {
            //this.pos = pos
        } */

        public abstract void Render();
        public abstract void Update();
    }
}