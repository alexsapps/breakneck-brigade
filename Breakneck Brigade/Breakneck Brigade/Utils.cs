using SousChef;
using System;
using System.Collections.Generic;
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
                default:
                    throw new Exception("GetGameObjectType not defined for " + objClass.ToString());
            }
        }
    }
}
