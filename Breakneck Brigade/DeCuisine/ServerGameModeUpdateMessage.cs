﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SousChef;

namespace DeCuisine
{
    public class ServerGameModeUpdateMessage : ServerMessage
    {
        public GameMode Mode { get; set; }
    }
}