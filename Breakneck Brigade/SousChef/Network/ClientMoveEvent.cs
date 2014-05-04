using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public class ClientMoveEvent : ClientEvent
    {
        public override ClientEventType Type
        {
            get { return ClientEventType.BeginMove; }
            set { throw new NotSupportedException(); }
        }
        public Coordinate Delta { get; set; }
    }
}
