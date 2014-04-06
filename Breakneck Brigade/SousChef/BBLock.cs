using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SousChef
{
    public class BBLock
    {
        public BBLock()
        {

        }

        public void AssertHeld()
        {
            Assert.AssertTrue(Monitor.IsEntered(this), "Lock not held.");
        }
    }
}
