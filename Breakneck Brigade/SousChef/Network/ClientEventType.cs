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
        /// Params: none.
        /// </summary>
        None, /* */
        /// <summary>
        /// Player changed orientation
        /// Params: "Orientation" -> a float representing the player's current orientation. Some degree angle from 0.
        /// </summary>
        ChangeOrientation,
        /// <summary>
        /// Player started moving\
        /// </summary>
        BeginMove,
        /// <summary>
        /// Player stopped moving
        /// </summary>
        EndMove,
        GrabItem,
        /// <summary>
        /// Throw a physics object
        /// Params:
        /// "Id" -> object ID
        /// "Force" -> a float[3] representing a force (direction + magnitude)
        /// To drop an object, throw it with a force of 0.
        /// </summary>
        ThrowItem,
        /// <summary>
        /// Player jumped
        /// Params:
        /// "Force" -> a float[3] representing a force (direction + magnitude)
        /// </summary>
        Jump,
        /// <summary>
        /// A test event. Forces the server to generate an object at a predefined space
        /// </summary>
        Test,

        /* below values are used by the server internally and should not be sent by the client */
        Enter,
        Leave
    }
}
