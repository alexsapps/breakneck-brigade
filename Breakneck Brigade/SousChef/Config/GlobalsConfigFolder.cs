using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SousChef
{
    public class GlobalsConfigFolder
    {
        public string ConfigDir { get; private set; }

        public GlobalsConfigFolder()
            : this(BBXml.GetAppConfigFolder())
        {

        }
        public GlobalsConfigFolder(string ConfigDir)
        {
            this.ConfigDir = ConfigDir;
        }

        public GlobalsConfigFile Open(string filename)
        {
            return new GlobalsConfigFile(this, BBXml.CombinePath(ConfigDir, filename));
        }
    }
}
