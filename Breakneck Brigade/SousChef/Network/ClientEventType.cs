using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public enum ClientEventType
    {
        ChangeOrientation,
        BeginMove,
        EndMove,
        GrabItem,
        ThrowItem, /* for drop item, make throw item event with 0 force */
        Jump,

        /* below values are used by the server internally and should not be sent by the client */
        Enter,
        Leave
    }
}
