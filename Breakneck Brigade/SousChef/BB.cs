using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static GeometryInfo GetPlayerGeomInfo()
        {
            return new GeometryInfo() { Mass = 40, Shape = GeomShape.Box, Sides = BB.GetPlayerSides(), Friction = 1.0f, Restitution = 0.2f };
        }
        public static float[] GetPlayerSides()
        {
            return new float[] { 6.0f, 6.0f, 6.0f };
        }
    }
}
