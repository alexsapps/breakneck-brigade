using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public enum ClientEventType
    {
        /// <summary>
        /// Null or Error 
        /// </summary>
        None, /* */
        /// <summary>
        /// 
        /// </summary>
        ChangeOrientation,  
        BeginMove,
        EndMove,
        GrabItem,
        ThrowItem, /* for drop item, make throw item event with 0 force */
        Jump,
        Test, //For testing purposes. Forces the server to spawn an item.

        /* below values are used by the server internally and should not be sent by the client */
        Enter,
        Leave
    }
}
