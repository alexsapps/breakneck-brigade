using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using grendgine_collada;

namespace Breakneck_Brigade.Graphics
{
    /*
     * A parser which pulls the bare minimum from Collada files to parse a Model object
     * Ryan George
     */
    class ColladaParser
    {
        public static Model parse(string filename)
        {
            Model parsedModel = new Model();
            Grendgine_Collada parsedFile = Grendgine_Collada.Grendgine_Load_File(filename);
            /*
             * Things we're interested in from collada files:
             * - Primatives (as collections of verts)
             *   In Collada: 
             *   Verts:     library_geometries->geometry->mesh->source#id="xxxx-mesh-positions"->float_array = array of vert positions, ordered using "technique_common" node 
             *   Normals:   library_geometries->geometry->mesh->source#id="xxxx-mesh-positions"->float_array
             * - Textures on primatives
             * - Relationships between prims
             */
            //parsedFile.Library_Geometries.Geometry.
            return parsedModel;
        }
    }
}
