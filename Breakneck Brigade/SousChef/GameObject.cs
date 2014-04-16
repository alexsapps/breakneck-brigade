using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace SousChef
{
    public abstract class GameObject
    {
        public int Id { get; set; }
        public Vector4 Transform { get; set; }

        public GameObject(int id)
        {
            this.Id = id;
            this.Transform = new Vector4();
        }

         public GameObject(int id, Vector4 transform)
         : this(id)
        {
            this.Transform = transform;
        } 

        public abstract void Update();
    }
}