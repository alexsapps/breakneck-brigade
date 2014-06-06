using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SousChef;

using BulletSharp;

namespace DeCuisine
{
    class ServerStaticObject : ServerGameObject
    {
        public override GameObjectClass ObjectClass { get { return GameObjectClass.StaticObject; } }
        public string Model { get; set; } // don't need to render the model, but a convinient way to store type
        public string FriendlyName { get; set; }
        private GeometryInfo _geomInfo;
        protected override GeometryInfo getGeomInfo() { return _geomInfo; }
        public string Team { get; set; }


        public override int SortOrder { get { return 0; } }

        public ServerStaticObject(ServerGame game, GeometryInfo geomInfo, string model, string friendlyName, Vector3 position, string team)
            : base(game)
        {
            this.Model = model;
            this.FriendlyName = friendlyName;
            this.Team = team;
            this._geomInfo = geomInfo;
            base.AddToWorld(position);
        }

        public override void Update()
        {
            
        }

        public override void Serialize(System.IO.BinaryWriter stream)
        {
            base.Serialize(stream);
            stream.Write(this.Model);
            stream.Write(this.FriendlyName);
            stream.Write(this.Team);
            stream.Write(GeomInfo.Size.Length);
            for (int i = 0; i < GeomInfo.Size.Length; i++)
            {
                stream.Write(GeomInfo.Size[i]);
            }
        }

        public override void UpdateStream(System.IO.BinaryWriter stream)
        {
            base.UpdateStream(stream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        public override void OnCollide(ServerGameObject obj)
        {
            base.OnCollide(obj);
            if (Model == "deliveryWindow" && obj.ObjectClass == GameObjectClass.Ingredient)
            {
                // trying to score, pass to the controller
                if(this.Game.Controller.ScoreDeliver((ServerIngredient)obj))
                {
                    this.Game.SendSound(BBSound.eatfood, this.Position);
                }
            }
            // hacky...
            if (this.FriendlyName == "Power Up Window" && obj.ObjectClass == GameObjectClass.Ingredient)
            {
                this.Game.Controller.powerUpItem((ServerIngredient)obj, this);
            }

            if(this.FriendlyName == "DJ Turntable" && obj.ObjectClass == GameObjectClass.Ingredient)
            {
     
                // THIS SHOULD CHANGE THE MUSIC
                Console.WriteLine("Should be changing the music!");
                
            }
        } 
    }
}
