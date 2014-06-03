using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    public enum BBParticleEffect
    {
        Effect1,
        Effect2
    }
    [Flags]
    public enum SmokeType
    {
        GREY = 1,
        WHITE = 2,
        BLUE = 4,
        RED = 8,
        GREEN = 16,
        YELLOW = 32
    }
}
