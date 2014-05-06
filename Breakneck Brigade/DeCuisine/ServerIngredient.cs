using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using System.IO;
using Tao.Ode;

namespace DeCuisine
{
    class ServerIngredient : ServerGameObject, IIngredient 
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Ingredient; } }
        public IngredientType Type { get; set; }
        public int Cleanliness { get; set; }
        protected override GeometryInfo getGeomInfo() { return this.Game.Config.Ingredients[Type.Name].GeomInfo; }

        /// <summary>
        /// Create the object and add it to the game world at vector4. Also added it to the 
        /// serial stream passed in.
        /// </summary>
        public ServerIngredient(IngredientType type, ServerGame game, Ode.dVector3 position) 
            : base(game)
        {
            this.Type = type;
            base.AddToWorld(position);
        }

        /// <summary>
        /// Write the intial creation packet for this object. Everything above the ==== 
        /// is handled in the base class. Below is handled by this class.
        /// 
        /// int32  id
        /// int16  objectclass
        /// float  X
        /// float  Y
        /// float  Z
        /// ===========
        /// string name of the type of ingredient. 
        /// int32  cleanliness
        /// </summary>
        /// <param name="stream"></param>
        public override void Serialize(BinaryWriter stream)
        { 
            base.Serialize(stream);
            stream.Write(this.Type.Name);
            stream.Write((Int32)this.Cleanliness);
        }

        /// <summary>
        /// Update the stream with the needed info. The id and position are handled by the
        /// base class. The packet will look as follows. Everything under ==== is handled
        /// by the base class
        /// 
        /// int32  id
        /// double x
        /// double y
        /// double z
        /// ==========
        /// int16  cleanliness
        /// </summary>
        /// <param name="stream"></param>
        public override void UpdateStream(BinaryWriter stream)
        {
            base.UpdateStream(stream);
            stream.Write((Int16)this.Cleanliness);
        }

        public override void Update()
        {
            Console.WriteLine("Before simluation " + this.Position.X + " " + this.Position.Y + " " + this.Position.Z);
            base.Update(); 
            Console.WriteLine("After simluation " + this.Position.X + " " + this.Position.Y + " " + this.Position.Z);
            
            
        }

        
    }
}
