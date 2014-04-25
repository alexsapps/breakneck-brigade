using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SousChef
{
    public class GameObjectType
    {
        /// <summary>
        /// Get or set the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the geometric information.
        /// </summary>
        public GeometryInfo GeomInfo { get; set; }

        public GameObjectType()
        {

        }

        public GameObjectType(string name, GeometryInfo geomInfo)
        {
            this.Name = name;
            this.GeomInfo = geomInfo;
        }
    }
}
