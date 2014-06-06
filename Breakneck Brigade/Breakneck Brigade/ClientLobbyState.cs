using SousChef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Breakneck_Brigade
{
    class ClientLobbyState
    {
        public Dictionary<string, ClientTeam> Teams = new Dictionary<string, ClientTeam>(); //team names/clients/scores are updated by ServerLobbyStateUpdateMessage, but scores are also updated by ServerTeamScoreMessage
        public GameMode Mode { get; set; } //updated by ServerGameModeUpdateMessage

        //updated by ServerLobbyStateUpdateMessage
        public TimeSpan MaxTime { get; set; }
        public int MaxScore { get; set; }
        public ClientTeam WinningTeam { get; set; }
        public ClientTeam MyTeam { get; set; }

        public bool IsDraw { get { if (Mode != GameMode.Results) { throw new Exception(); } return WinningTeam == null; } }
        public bool IWin { get { if (Mode != GameMode.Results) { throw new Exception(); } return (WinningTeam == MyTeam) && WinningTeam != null; } }


    }
}
