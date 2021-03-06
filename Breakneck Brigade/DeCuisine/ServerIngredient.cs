﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;
using System.IO;

using BulletSharp;

namespace DeCuisine
{
    class ServerIngredient : ServerGameObject, IIngredient 
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.Ingredient; } }
        public IngredientType Type { get; set; }
        public int Cleanliness { get; set; }
        protected override GeometryInfo getGeomInfo() { return this.Game.Config.Ingredients[Type.Name].GeomInfo; }

        public ServerPlayer LastPlayerHolding { get; set; }

        public override int SortOrder { get { return 0; } }

        /// <summary>
        /// Create the object and add it to the game world at vector4. Also added it to the 
        /// serial stream passed in.
        /// </summary>
        public ServerIngredient(IngredientType type, ServerGame game, Vector3 position) 
            : base(game)
        {
            this.Type = type;
            base.AddToWorld(position);
            LastPlayerHolding = null;
            Cleanliness = 100;
            this.Body.ActivationState = ActivationState.ActiveTag;
            this.Body.ActivationState = ActivationState.DisableDeactivation;
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
            LastPlayerHolding = null; // no player is holding it. 
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

        protected override void updateHook()
        {
            var lastPos = this.Position;
        }
        public override void OnCollide(ServerGameObject obj)
        {
            base.OnCollide(obj);
        }
        
    }
}
