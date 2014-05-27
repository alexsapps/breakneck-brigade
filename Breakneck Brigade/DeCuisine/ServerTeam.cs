using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeCuisine
{
    class ServerTeam
    {
        private List<Client> _members;
        
        public int Points { get; set; }
        public string Name { get; set; }
        public List<string> TintList{ get; set; }

        public ServerTeam(string name)
        {
            _members = new List<Client>();
            this.Name = name;
            this.TintList = new List<string>();
        }

        public List<Client> GetMembers()
        {
            return new List<Client>(_members);
        }
        public void AddMember(Client member)
        {
            _members.Add(member);
            member.Disconnected += member_Disconnected;
        }
        public void RemoveMember(Client member)
        {
            _members.Remove(member);
            member.Disconnected -= member_Disconnected;
        }
        void member_Disconnected(object sender, EventArgs e)
        {
            Debug.Assert(_members.Contains((Client)sender));
            _members.Remove((Client)sender);
        }
    }
}
