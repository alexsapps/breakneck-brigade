using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SousChef
{
    public class GameObjectType
    {
        /// <summary>
        /// Get or set the code-name.  Should be camel case.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the friendly name.  May be displayed to the user.  May contain spaces.  Should be capitalized properly.
        /// </summary>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the geometric information.
        /// </summary>
        public GeometryInfo GeomInfo { get; set; }

        public GameObjectType()
        {

        }

        public GameObjectType(string name, string friendlyName, GeometryInfo geomInfo)
        {
            this.Name = name;
            this.FriendlyName = friendlyName;
            this.GeomInfo = geomInfo;
        }
    }
}
