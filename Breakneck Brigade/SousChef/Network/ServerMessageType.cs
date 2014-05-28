using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public enum ServerMessageType
    {
        GameModeUpdate,
        GameStateUpdate,
        PlayerIdUpdate,
        ServerCommandResponse,
        ServerLobbyStateUpdateMessage,
        ServerTeamScoreMessage
    }
}
