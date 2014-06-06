using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using SousChef;
namespace DeCuisine
{
    class ServerTeam
    {
        private List<Client> _members;
        
        public int Points { get; set; }
        public string Name { get; set; }
        public Dictionary<ServerCooker,List<string>> HintHash{ get; set; }
        public Vector3 SpawnPoint;

        public ServerTeam(string name, Vector3 spawnPoint)
        {
            this._members = new List<Client>();
            this.Name = name;
            this.HintHash = new Dictionary<ServerCooker,List<string>>();
            this.SpawnPoint = spawnPoint;
        }

        public List<Client> GetMembers()
        {
            return new List<Client>(_members);
        }

        public void AddMember(Client member)
        {
            _members.Add(member);
            if (member.Player != null)
            {
                member.Player.Team = this;
            }

            member.Disconnected += member_Disconnected;
        }
        public void RemoveMember(Client member)
        {
            _members.Remove(member);
            if (member.Player != null)
            {
                member.Player.Team = null;
            }

            member.Disconnected -= member_Disconnected;
        }

        void member_Disconnected(object sender, EventArgs e)
        {
            Debug.Assert(_members.Contains((Client)sender));
            _members.Remove((Client)sender);
        }

        /// <summary>
        /// Loop over all the lists in the hint hash and returns a list of 
        /// unique string values of the ingredients to hint for this team
        /// </summary>
        public List<string> getTintList()
        {
            HashSet<string> tmpHash = new HashSet<string>();

            foreach (var tmpList in HintHash.Values.ToList())
            {
                foreach (var ing in tmpList)
                {
                    tmpHash.Add(ing);
                }
            }
            return tmpHash.ToList();
        }
    }
}
