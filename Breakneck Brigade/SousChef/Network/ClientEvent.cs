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
        public abstract ClientEventType Type { get; }

        public abstract void Write(BinaryWriter writer);
    }
}
