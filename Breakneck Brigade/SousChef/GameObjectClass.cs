using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    /*
     *     * warning *
     *     
     *     if adding items here, be sure to update
     *     Breakneck_Brigade.Utils.GetGameObjectType(this GameObjectClass)
     */

    public enum GameObjectClass
    {
        Player,
        Ingredient,
        Cooker,
        Terrain,
        StaticObject
    }
}
