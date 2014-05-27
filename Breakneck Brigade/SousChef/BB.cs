using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef.MPNamespace;

namespace SousChef
{
    public static partial class BB
    {
        public const string ProtocolNo = "1"; //increment when protocol changes
        public const string ServerProtocolHandshakeStr = "BreakneckBrigadeSrvr." + ProtocolNo; //server sends this
        public const string ClientProtocolHandshakeStr = "BreakneckBrigadeClnt." + ProtocolNo; //client sends this
        public const string DefaultServerHost = "127.0.0.1";
        public const int DefaultServerPort = 2222;
        public static string GlobalConfigFilename = "global-config.xml";
        public const string DefaultPlayerModel = "teapotPillar";

        public static ModelParser modelParser = new ModelParser();

        public static GeometryInfo GetPlayerGeomInfo()
        {
            var vertMinMax = modelParser.ScaleVector[DefaultPlayerModel];
            float[] modelScale = new float[] { (vertMinMax[1].X - vertMinMax[0].X) / 2, (vertMinMax[1].Y - vertMinMax[0].Y) / 2, (vertMinMax[1].Z - vertMinMax[0].Z) / 2 };
            return new GeometryInfo() { Mass = 40, ModelScale = modelScale, Shape = GeomShape.Box, Size = BB.GetPlayerSides(), Friction = 1.0f, Restitution = 0.2f };
        }
        public static float[] GetPlayerSides()
        {
            return new float[] { 6.0f, 3.0f, 6.0f };
        }
    }
}
