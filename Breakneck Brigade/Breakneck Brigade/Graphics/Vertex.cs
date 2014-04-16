using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

namespace Breakneck_Brigade.Graphics
{
    /*
     * Holds data about a vertex, namely the location in model space and its normal;
     */
    class Vertex
    {
        public Vector4 Position;
        public Vector4 Normal;
        public Vector4 TextureCoordinates;

        public Vertex()
        {
            this.Position           = new Vector4();
            this.Normal             = new Vector4();
            this.TextureCoordinates = new Vector4();
        }

        public Vertex(Vector4 pos, Vector4 norm, Vector4 tc)
        {
            this.Position           = pos;
            this.Normal             = norm;
            this.TextureCoordinates = tc;
        }

        public Vertex(float posX, float posY, float posZ, float normX, float normY, float normZ, float tcX, float tcY, float tcZ)
        {
            this.Position               = new Vector4(posX, posY, posZ);
            this.Normal                 = new Vector4(normX, normY, normZ);
            this.TextureCoordinates     = new Vector4(tcX, tcY, tcZ);
        }
        public Vertex(double posX, double posY, double posZ, double normX, double normY, double normZ, double tcX, double tcY, double tcZ)
        {
            this.Position               = new Vector4(posX, posY, posZ);
            this.Normal                 = new Vector4(normX, normY, normZ);
            this.TextureCoordinates     = new Vector4(tcX, tcY, tcZ);
        }
    }
}
