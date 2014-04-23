using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    /// <summary>
    /// Holds all the information necessary for physics interaction.
    /// </summary>
    public class GeometryInfo
    {
        /// <summary>
        /// Gets or sets the mass of the object
        /// </summary>
        public float Mass { get; set; }

        /// <summary>
        /// Gets or sets the shpae of the object
        /// </summary>
        public GeomShape Shape { get; set; }

        /// <summary>
        /// Gets the list of sides of the object
        /// </summary>
        public List<float> Sides { get; protected set; }

        public GeometryInfo()
        {
            this.Sides = new List<float>();
        }
    }
}
