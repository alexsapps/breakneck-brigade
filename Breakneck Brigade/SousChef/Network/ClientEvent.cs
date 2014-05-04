using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SousChef;
using System.IO;

namespace SousChef
{
    public abstract class ClientEvent
    {
        public virtual ClientEventType Type { get; set; }
    }
}
