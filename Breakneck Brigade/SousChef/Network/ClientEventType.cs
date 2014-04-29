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
        /// Type to used when a message is empty. Indicates null.
        /// Args: none.
        /// </summary>
        None, /* */
        /// <summary>
        /// Player changed orientation
        /// Args: "Orientation" -> a float representing the player's current orientation. Some degree angle from 0.
        /// </summary>
        ChangeOrientation,
        /// <summary>
        /// Player started moving
        /// Args: "Direction" -> a float[3] representing direction
        /// </summary>
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
