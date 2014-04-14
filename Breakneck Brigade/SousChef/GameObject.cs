using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SousChef
{
    public abstract class GameObject
    {
        public int Id { get; set; }
        public Vector4 Position { get; set; }

        public GameObject(int id)
        {
            this.Id = id;
            this.Position = new Vector4();
        }

         public GameObject(int id, Vector4 position)
         : this(id)
        {
            this.Position = Position;
        } 

        public abstract void Update();
    }
}