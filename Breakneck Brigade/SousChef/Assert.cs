using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SousChef
{
    class Assert
    {
        public static void AssertTrue(bool x, string message)
        {
            if (!x)
                throw new Exception("Assertion failed: " + message);
        }
    }
}
