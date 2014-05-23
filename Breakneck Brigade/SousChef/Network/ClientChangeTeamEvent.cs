using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ClientChangeTeamEvent : ClientEvent
    {
        public string TeamName;

        public override ClientEventType Type
        {
            get { return ClientEventType.ChangeTeam; }
        }

        public ClientChangeTeamEvent() { }

        public ClientChangeTeamEvent(BinaryReader reader)
        {
            this.TeamName = reader.ReadString();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(TeamName);
        }
    }
}
