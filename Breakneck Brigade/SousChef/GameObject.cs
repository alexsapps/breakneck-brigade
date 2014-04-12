using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SousChef
{
    public abstract class GameObject
    {
        public int id { get; set; }
        public Vector4 pos { get; set; }

        public GameObject(int id)
        {
            this.id = id;
            this.pos = new Vector4();
        }

         public GameObject(int id, Vector4 position)
         : this(id)
        {
            this.pos = pos;
        } 

        public abstract void Update();
    }
}