using SousChef;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breakneck_Brigade
{
    static class Utils
    {
        public static Type GetGameObjectType(this GameObjectClass objClass)
        {
            switch (objClass)
            {
                case GameObjectClass.Ingredient:
                    return typeof(ClientIngredient);
                case GameObjectClass.Cooker:
                    return typeof(ClientCooker);
                case GameObjectClass.Plane:
                    return typeof(ClientPlane);
                case GameObjectClass.Box:
                    return typeof(ClientBox);
                default:
                    throw new Exception("GetGameObjectType not defined for " + objClass.ToString());
            }
        }

        public static Vector4 ReadCoordinate(this BinaryReader stream)
        {
            return new Vector4(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
        }
    }
}
