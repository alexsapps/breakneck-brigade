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
                case GameObjectClass.Terrain:
                    return typeof(ClientTerrain);
                case GameObjectClass.StaticObject:
                    return typeof(ClientStaticObject);
                case GameObjectClass.Player:
                    return typeof(ClientPlayer);
                default:
                    throw new Exception("GetGameObjectType not defined for " + objClass.ToString());
            }
        }

        public static Vector4 ReadCoordinate(this BinaryReader stream)
        {
            return new Vector4(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle());
        }

        public static Matrix4 ReadRotation(this BinaryReader stream)
        {
            return new Matrix4(stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), 0,
                               stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), 0,
                               stream.ReadSingle(), stream.ReadSingle(), stream.ReadSingle(), 0,
                               0,                   0,                   0,                   1);
        }

        public static void Write(this BinaryWriter writer, Vector4 vector)
        {
            writer.Write(vector.X);
            writer.Write(vector.Z); //need to switch Z and Y for opengl-->ode
            writer.Write(vector.Y);
        }
    }
}
