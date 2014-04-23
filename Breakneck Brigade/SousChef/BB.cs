using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class BB
    {
        public const string ProtocolNo = "1"; //increment when protocol changes
        public const string ServerProtocolHandshakeStr = "BreakneckBrigadeSrvr." + ProtocolNo; //server sends this
        public const string ClientProtocolHandshakeStr = "BreakneckBrigadeClnt." + ProtocolNo; //client sends this
        public const string DefaultServerHost = "127.0.0.1";
        public const int DefaultServerPort = 2222;
        public static string GlobalConfigFilename = "global-config.xml";

        
    }
}
