using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SousChef;

namespace SousChef
{
    public class ClientEvent
    {
        public ClientEventType Type { get; set; }
        public Dictionary<string, string> Args { get; set; }
        public ClientEvent()
        {
            Args = new Dictionary<string, string>();
        }

        public static ClientEvent buildEndMoveEvent()
        {
            ClientEvent ce = new ClientEvent();
            ce.Type = ClientEventType.EndMove;
            return ce;
        }
        
        public static ClientEvent buildBeginMoveEvent()
        {
            ClientEvent ce = new ClientEvent();
            ce.Type = ClientEventType.BeginMove;
            return ce;
        }

        public static ClientEvent buildChangeOrientationEvent(float orientation)
        {
            ClientEvent ce = new ClientEvent();
            ce.Type = ClientEventType.ChangeOrientation;
            ce.Args.Add("Orientation", orientation.ToString());
            return ce;
        }
    }
}
