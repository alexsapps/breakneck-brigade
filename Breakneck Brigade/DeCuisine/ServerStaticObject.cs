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
        private GeometryInfo _geomInfo;
        protected override GeometryInfo getGeomInfo() { return _geomInfo; }

        public override int SortOrder { get { return 0; } }

        public ServerStaticObject(ServerGame game, GeometryInfo geomInfo, string model, Vector3 position)
            : base(game)
        {
            this.Model = model;
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
            
            stream.Write(GeomInfo.Sides.Length);
            for (int i = 0; i < GeomInfo.Sides.Length; i++)
            {
                stream.Write(GeomInfo.Sides[i]);
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
            if (Model == "deleveryWindow")
            {

            }
        }
    }
}
